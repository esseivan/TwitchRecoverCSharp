/*
 * Copyright (c) 2020, 2021 Daylam Tayari <daylam@tayari.gg>
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License version 3as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not see http://www.gnu.org/licenses/ or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 *
 *  @author Daylam Tayari daylam@tayari.gg https://github.com/daylamtayari
 *  @version 2.0aH     2.0a Hotfix
 *  Github project home page: https://github.com/TwitchRecover
 *  Twitch Recover repository: https://github.com/TwitchRecover/TwitchRecover
 */

using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchRecoverCs.core.Downloader;
using static System.Net.WebRequestMethods;

namespace TwitchRecoverCs.core.Downloader
{
    /**
     * This class holds all of the downloading methods
     * and handles all of the downloads.
     */
    public class Download
    {
        public static event EventHandler<DownloadProgressChangedEventArgs> ProgressChanged;
        public static event EventHandler<int> DownloadStarted;
        public static event EventHandler<int> ChunkDownloaded;
        public static int P_MAX_COUNT = 0;
        public static int P_COUNT = 0;

        private const int MAX_TRIES = 5;
        /**
         * This method downloads a file from a
         * given URL and downloads it at a given
         * file path.
         * @param url           string value representing the URL to download.
         * @param fp            string value representing the complete file path of the file.
         * @return string       string value representing the complete file path of where the file was downloaded.
         * @throws IOException
         */
        public static string download(string url, string fp)
        {
            string extension = url.Substring(url.LastIndexOf("."));
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(url, fp + extension);
            }
            return fp + extension;
        }

        private static SortedDictionary<int, FileDestroyer> segmentMap = null;
        public async static Task<string> m3u8Download(string url, string fp)
        {
            FileHandler.createTempFolder();
            List<string> chunks = M3U8Handler.getChunks(url);
            DownloadStarted?.Invoke(url, chunks.Count);
            segmentMap = await TSDownload(chunks);
            string result = FileHandler.mergeFile(segmentMap, fp);
            return result;
        }

        public static string m3u8_retryMerge(string fp)
        {
            try
            {
                string result = FileHandler.mergeFile(segmentMap, fp);
                return result;
            }
            catch (IOException) { }
            return string.Empty;
        }

        /**
         * This method creates a temporary download
         * from a URL.
         * @param url       URL of the file to be downloaded.
         * @return File     File object of the file that will be downloaded and is returned.
         * @throws IOException
         */
        public static async Task<FileDestroyer> tempDownloadAsync(string url)
        {
            string prefix = Path.GetFileNameWithoutExtension(url);

            if (prefix.Length < 2)
            {     //This has to be implemented since the prefix value of the createTempFile method
                prefix = "00" + prefix;     //which we use to create a temp file, has to be a minimum of 3 characters long.
            }
            else if (prefix.Length < 3)
            {
                prefix = "0" + prefix;
            }

            FileDestroyer downloadedFile = FileHandler.createTempFile(prefix + "-", Path.GetExtension(url));    //Creates the temp file.
            using (WebClient wc = new WebClient())
            {
                await wc.DownloadFileTaskAsync(new Uri(url), downloadedFile);
            }

            return downloadedFile;
        }

        /**
         * This method creates a temporary download
         * from a URL.
         * @param url       URL of the file to be downloaded.
         * @return File     File object of the file that will be downloaded and is returned.
         * @throws IOException
         */
        public static string tempDownload(string url)
        {
            string prefix = Path.GetFileNameWithoutExtension(url);

            if (prefix.Length < 2)
            {     //This has to be implemented since the prefix value of the createTempFile method
                prefix = "00" + prefix;     //which we use to create a temp file, has to be a minimum of 3 characters long.
            }
            else if (prefix.Length < 3)
            {
                prefix = "0" + prefix;
            }

            string downloadedFile = FileHandler.createTempFile(prefix + "-", Path.GetExtension(url));    //Creates the temp file.
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(new Uri(url), downloadedFile);
            }

            return downloadedFile;
        }

        /**
         * This method downloads all of the segments
         * of an M3U8 playlist and incorporates them all
         * in a navigable map.
         * @param links                         Arraylist holding all of the links to download.
         * @return NavigableMap<Integer, File>  Navigable map holdding the index and file objects of each TS segment.
         */
        private async static Task<SortedDictionary<int, FileDestroyer>> TSDownload(List<string> links)
        {
            SortedDictionary<int, FileDestroyer> segmentMap = new SortedDictionary<int, FileDestroyer>();
            ConcurrentQueue<string> downloadQueue = new ConcurrentQueue<string>(links);
            ConcurrentQueue<FileDestroyer> downloadedQueue = new ConcurrentQueue<FileDestroyer>();

            int index = 0;
            while (!downloadQueue.IsEmpty)
            {
                index++;

                int finalIndex = index;

                // Downloader task
                await Task.Factory.StartNew(async () =>
                {
                    int currentTries = 1;
                    if (downloadQueue.TryDequeue(out string item))
                    {
                        while (currentTries <= MAX_TRIES)
                        {
                            int threadIndex = finalIndex;
                            string threadItem = item;
                            try
                            {
                                FileDestroyer tempTS = await tempDownloadAsync(threadItem);
                                segmentMap[threadIndex] = tempTS;
                                ChunkDownloaded?.Invoke(tempTS, threadIndex);
                                // This thread is done
                                downloadedQueue.Enqueue(tempTS);
                                return; // WARN : return here
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(string.Format("WARNING : Unable to download {0}. Reason is :", item));
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                    // IF an error occured, input a invalid one
                    downloadedQueue.Enqueue(new FileDestroyer(item));
                });
            }

            while (downloadedQueue.Count != links.Count)
            {
                Console.WriteLine(string.Format("Waiting for download... Currently at {0}/{1}", downloadedQueue.Count, links.Count));
                await Task.Delay(100);
            }

            return segmentMap;
        }
    }
}
