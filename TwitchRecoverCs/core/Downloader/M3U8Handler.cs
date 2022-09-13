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
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchRecoverCs.core.Downloader;

namespace TwitchRecoverCs.core.Downloader
{
    /**
     * This class handles all of the methods related
     * to M3U8 processing for downloads.
     */
    public class M3U8Handler
    {
        /**
     * This method retrieves all of the
     * chunks of an M3U8 URL.
     * @param url                   URL of the M3U8 file to retrieve the chunks off.
     * @return List<string>    string arraylist containing all of the chunks of the M3U8 file.
     * @throws IOException
     */
        public static List<string> getChunks(string url)
        {
            List<string> chunks = new List<string>();
            string baseURL = "";
            string pattern = "([a-z0-9\\-]*.[a-z_]*.[net||com||tv]*\\/[a-z0-9_]*\\/[a-zA-Z0-9]*\\/)index[0-9a-zA-Z-]*.m3u8";
            Regex r = new Regex(pattern);
            var m = r.Match(url);

            if (m.Success)
            {
                baseURL = "https://" + m.Groups[1];
            }
            string pattern2 = "([a-z0-9\\-]*.[a-z_]*.[net||com||tv]*\\/[a-zA-Z0-9_]*\\/[0-9]*\\/[a-zA-Z0-9_-]*\\/[0-9p]*\\/)";
            Regex r2 = new Regex(pattern2);
            var m2 = r.Match(url);
            if (m2.Success)
            {
                baseURL = "https://" + m2.Groups[1];
            }
            string m3u8File = Download.tempDownload(url);

            using (StreamReader sr = new StreamReader(m3u8File))
            {
                string line;
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (line.Contains("unmuted") && !line.StartsWith("#"))
                    {
                        chunks.Add(baseURL + line.Substring(0, line.IndexOf("-") + 1) + "muted.ts");
                    }
                    else if (!line.StartsWith("#"))
                    {
                        chunks.Add(baseURL + line);
                    }
                }
            }
            return chunks;
        }
    }
}
