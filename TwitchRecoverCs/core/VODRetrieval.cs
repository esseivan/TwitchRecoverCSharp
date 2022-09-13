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
using TwitchRecoverCs.core.API;
using TwitchRecoverCs.core.Downloader;

namespace TwitchRecoverCs.core
{
    /**
     * The VOD retrieval class is the class that orchestrates
     * all of the VOD retrieval and is what is called by the
     * CLI and GUI packages.
     */
    public class VODRetrieval
    {
        /**
     * This method retrieves the VOD M3U8
     * URLs from given string values.
     * @param name                  string value representing the streamer's name.
     * @param sID                   string value representing the stream ID.
     * @param ts                    string value representing the timestamp of the stream.
     * @param bf                    Boolean value which represents whether a VOD brute force should be carried out.
     * @return List<string>    string arraylist which represents all of the working VOD M3U8 URLs.
     */
        public static List<string> retrieveVOD(string name, string sID, string ts, bool bf)
        {
            List<string> results = new List<string>();
            long timestamp = Compute.getUNIX(ts);
            long streamID = long.Parse(sID);
            if (bf)
            {
                results = Fuzz.BFURLs(name, streamID, timestamp);
            }
            else
            {
                string url = Compute.URLCompute(name, streamID, timestamp);
                results = Fuzz.verifyURL(url);
            }
            return results;
        }

        /**
         * This method retrieves all
         * of the possible feeds of a
         * VOD and returns a Feeds object
         * containing them.
         * @param baseURL   string value representing the base, source URL to check all of the qualities for.
         * @return Feeds    Feeds object containing all of the possible feeds for that particular VOD.
         */
        public static Feeds retrieveVODFeeds(string baseURL)
        {
            string coreURL = Compute.singleRegex("(https:\\/\\/[a-z0-9\\-]*.[a-z_]*.[net||com||tv]*\\/[a-z0-9_]*\\/)chunked\\/index-dvr.m3u8", baseURL);
            return Fuzz.fuzzQualities(coreURL, "/index-dvr.m3u8");
        }

        /**
         * This method retrieves the VODID
         * from a complete VOD link.
         * @param url   Twitch VOD link (or raw ID) of a VOD.
         */
        public static long retrieveID(string url)
        {
            if (Compute.singleRegex("(twitch.tv\\/[a-z0-9_]*\\/v\\[0-9]*)", url) != null)
            {
                return long.Parse(Compute.singleRegex("twitch.tv\\/[a-zA-Z0-9_]*\\/v\\/([0-9]*)", url));
            }
            else if (Compute.singleRegex("(twitch.tv\\/[a-z0-9_]*\\/videos?\\/[0-9]*)", url) != null)
            {
                return long.Parse(Compute.singleRegex("twitch.tv\\/[a-z0-9_]*\\/videos?\\/([0-9]*)", url));
            }
            else if (Compute.singleRegex("(twitch.tv\\/videos\\/[0-9]*)", url) != null)
            {
                return long.Parse(Compute.singleRegex("twitch.tv\\/videos\\/([0-9]*)", url));
            }
            else
            {
                return long.Parse(url);
            }
        }

        /**
         * This method returns a bool
         * value depending on whether or
         * not an M3U8 has muted segments.
         * @param url       string value representing the URL of the M3U8 to check.
         * @return bool  Boolean value which is true if the M3U8 contains muted segments and false if otherwise.
         */
        public static bool hasMuted(string url)
        {
            string m3u8 = null;
            try
            {
                m3u8 = Download.tempDownload(url);
            }
            catch (IOException) { }
            List<string> contents = FileIO.read(m3u8);
            foreach (string line in contents)
            {
                if (line.Contains("unmuted"))
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * This method unmutes an M3U8.
         * @param value     string value representing either the file absolute path or the M3U8 URL.
         * @param isFile    Boolean value representing whether or not the M3U8 input value is a file (true value) or is a URL (false value).
         * @param outFP     string value representing the user's desired complete output file path.
         */
        public static void unmute(string value, bool isFile, string outFP)
        {
            string m3u8 = null;
            string url = "";
            if (isFile)
            {
                m3u8 = value;
            }
            else
            {
                try
                {
                    m3u8 = Download.tempDownload(value);
                }
                catch (Exception) { }
                url = value.Substring(0, value.IndexOf("index-dvr.m3u8"));
            }
            FileIO.write(unmuteContents(FileIO.read(m3u8), url), outFP);
        }

        /**
         * This method unmutes the contents of
         * an arraylist representing the values
         * of an M3U8 file.
         * @param contents              string arraylist representing the contents of the raw M3U8 file.
         * @param url                   string value representing the URL to add to each TS part if applicable. Can be empty.
         * @return List<string>    string arraylist containing the unmuted contents of the M3U8 file.
         */
        private static List<string> unmuteContents(List<string> contents, string url)
        {
            List<string> unmutedContents = new List<string>();
            foreach (string line in contents)
            {
                if (line.Contains("-unmuted.ts") && !line.StartsWith("#"))
                {
                    unmutedContents.Add(url + line.Substring(0, line.LastIndexOf("unmuted.ts")) + "muted.ts");
                }
                else if (!line.StartsWith("#"))
                {
                    unmutedContents.Add(url + line);
                }
                else
                {
                    unmutedContents.Add(line);
                }
            }
            return unmutedContents;
        }
    }
}
