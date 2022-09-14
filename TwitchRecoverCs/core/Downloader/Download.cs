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
using System.Net.Http;
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
        public async static Task<string> m3u8Download(string url, string fp, CancellationToken token)
        {
            FileHandler.createTempFolder();
            List<string> chunks = M3U8Handler.getChunks(url);
            DownloadStarted?.Invoke(url, chunks.Count);
            segmentMap = await TSDownload(chunks, token);
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
        public static async Task<FileDestroyer> tempDownloadAsync(string url, CancellationToken token)
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
                var downloadTask = wc.DownloadFileTaskAsync(new Uri(url), downloadedFile);
                using (token.Register(() => wc.CancelAsync()))
                {
                    await downloadTask;
                    token.ThrowIfCancellationRequested();
                }
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
        private async static Task<SortedDictionary<int, FileDestroyer>> TSDownload(List<string> links, CancellationToken token)
        {
            SortedDictionary<int, FileDestroyer> segmentMap = new SortedDictionary<int, FileDestroyer>();
            ConcurrentQueue<string> downloadQueue = new ConcurrentQueue<string>(links);
            ConcurrentQueue<FileDestroyer> downloadedQueue = new ConcurrentQueue<FileDestroyer>();

            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler?redirectedfrom=MSDN&view=net-6.0

            // Limit to x threads simultaneously
            SemaphoreSlim maxThread = new SemaphoreSlim(8);

            int index = 0;
            while (!downloadQueue.IsEmpty)
            {
                index++;
                int finalIndex = index;

                // Wait for room
                Console.WriteLine("{0} thread(s) available", maxThread.CurrentCount);
                await maxThread.WaitAsync();    // Wait for a thread to be available
                if (token.IsCancellationRequested)
                    return segmentMap; // Still output the current segment map

                // Downloader task
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                (await Task.Factory.StartNew(async () =>
                {
                    int currentTries = 1;
                    if (downloadQueue.TryDequeue(out string item)) // Get an url
                    {
                        int threadIndex = finalIndex;
                        string threadItem = item;
                        while (currentTries <= MAX_TRIES)
                        {
                            try
                            {
                                FileDestroyer tempTS = await tempDownloadAsync(threadItem, token);
                                segmentMap[threadIndex] = tempTS;   // save id and filepath
                                ChunkDownloaded?.Invoke(tempTS, threadIndex);
                                // This thread is done
                                downloadedQueue.Enqueue(tempTS);
                                return; // WARN : return here
                            }
                            catch (Exception ex)
                            {
                                if (token.IsCancellationRequested)
                                    return;
                                Console.WriteLine(string.Format("WARNING : Unable to download {0}. Reason is :", item));
                                Console.WriteLine(ex.ToString());
                            }
                            if (token.IsCancellationRequested)
                                return;
                        }
                    }
                    // If an error occured, input a invalid one
                    downloadedQueue.Enqueue(new FileDestroyer(item));
                }, token)).ContinueWith((task) => maxThread.Release());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            Console.WriteLine("All tasks created !!!");

            while (downloadedQueue.Count != links.Count)
            {
                Console.WriteLine(string.Format("Waiting for download... Currently at {0}/{1}", downloadedQueue.Count, links.Count));
                await Task.Delay(100);
                if (token.IsCancellationRequested)
                    return segmentMap; // Still output the current segment map
            }

            Console.WriteLine("\n\nSuccessful completion.");

            return segmentMap;
        }
    }

    // Provides a task scheduler that ensures a maximum concurrency level while
    // running on top of the thread pool.
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        // Indicates whether the current thread is processing work items.
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        // The list of tasks to be executed
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        // The maximum concurrency level allowed by this scheduler.
        private readonly int _maxDegreeOfParallelism;

        // Indicates whether the scheduler is currently processing work items.
        private int _delegatesQueuedOrRunning = 0;

        // Creates a new instance with the specified degree of parallelism.
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        // Queues a task to the scheduler.
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // Inform the ThreadPool that there's work to be executed for this scheduler.
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        // Attempts to execute the specified task on the current thread.
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                // Try to run the task.
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        // Attempt to remove a previously scheduled task from the scheduler.
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        // Gets the maximum concurrency level supported by this scheduler.
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        // Gets an enumerable of the tasks currently scheduled on this scheduler.
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }
}
