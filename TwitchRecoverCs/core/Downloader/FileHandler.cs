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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchRecoverCs.core.Downloader
{
    /**
     * This class handles all of the file handling
     * for the Download package.
     */
    internal class FileHandler
    {
        protected static string TEMP_FOLDER_PATH;    //Variable which holds the folder path of the temp folder.
        protected static FolderDestroyer FD = null;

        /**
         * This method creates a temp folder where all of the temporary TS
         * files (M3U8 parts) will be saved.
         * @throws IOException
         */
        protected static void createTempFolder()
        {
            if (FD != null)
                FD.Dispose();

            TEMP_FOLDER_PATH = Path.GetTempPath();
            string tempDirectory = Path.Combine(TEMP_FOLDER_PATH, string.Format("TwitchRecover-{0}", Path.GetRandomFileName().Split('.')[0]));
            Directory.CreateDirectory(tempDirectory);
            FD = new FolderDestroyer(tempDirectory);
        }

        /**
         * This method merges all of the
         * segmented files of the M3U8 playlist
         * into a single file.
         * @param segmentMap    Navigable map holding the index and file objects of all the segment files.
         * @param fp            Final file path of the file.
         */
        protected static string mergeFile(SortedDictionary<int, string> segmentMap, string fp)
        {
            using (var outputStream = File.Create(fp))
            {
                foreach (var inputFilePath in segmentMap)
                {
                    using (var inputStream = File.OpenRead(inputFilePath.Value))
                    {
                        // Buffer size can be passed as the second argument.
                        inputStream.CopyTo(outputStream);
                    }
                    Console.WriteLine("The file {0} has been processed.", inputFilePath.Value);
                }
            }
            return fp;
        }

        /// <summary>
        /// Solution to replace tempDir.deleteOnExit();
        /// </summary>
        protected class FolderDestroyer : IDisposable
        {
            protected readonly string Path;

            internal FolderDestroyer(string p)
            {
                Path = p;
            }

            ~FolderDestroyer()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (Directory.Exists(Path))
                    Directory.Delete(Path);
            }
        }
    }
}
