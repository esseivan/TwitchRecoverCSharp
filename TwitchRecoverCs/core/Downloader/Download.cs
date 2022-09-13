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
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchRecoverCs.core.Downloader;

namespace TwitchRecoverCs.core.Downloader
{
    /**
     * This class holds all of the downloading methods
     * and handles all of the downloads.
     */
    public class Download
    {
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

        public static string m3u8Download(string url, string fp)
        {
            FileHandler.createTempFolder();
            List<string> chunks = M3U8Handler.getChunks(url);
            SortedDictionary<int, string> segmentMap = TSDownload(chunks);
            return FileHandler.mergeFile(segmentMap, fp);
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

            string downloadedFile = FileHandler.createTempFile(prefix + "-", "." + Path.GetExtension(url));    //Creates the temp file.
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(url, downloadedFile);
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
        private static SortedDictionary<int, string> TSDownload(List<string> links)
        {
            SortedDictionary<int, string> segmentMap = new SortedDictionary<int, string>();
            ConcurrentQueue<string> downloadQueue = new ConcurrentQueue<string>(links);


            int index = 0;
            while (!downloadQueue.IsEmpty)
            {
                index++;

                int finalIndex = index;

                Task.Factory.StartNew(() =>
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
                                string tempTS = tempDownload(threadItem);
                                segmentMap[threadIndex] = tempTS;
                                break;
                            }
                            catch (Exception) { }
                        }
                    }
                });
            }

            return segmentMap;
        }
    }
}
