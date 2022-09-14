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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchRecoverCs.core.Enums;

namespace TwitchRecoverCs.core.API
{
    /**
     * This class contains the fuzzing methods and the
     * core fuzzing method which is called to find a clip.
     */
    public class Fuzz
    {
        /**
         * This is the core method for fuzzing all of the
         * clips of a particular stream.
         * @param streamID Long value which represents the stream ID for which clips should be fuzzed for.
         * @param duration Long value which represents the duration of the stream.
         * @param wfuzz    Boolean which represents whether Wfuzz is installed and should be used or not.
         * @return List<string>    string List which holds all of the results of clips.
         */
        public static List<string> fuzz(long streamID, long duration, bool wfuzz_val)
        {
            List<string> results = new List<string>();
            int reps = (((int)duration) * 60) + 2000;
            if (wfuzz_val)
            {
                results = wfuzz(streamID, reps);
            }
            else
            {
                results = jFuzz(streamID, reps);
            }
            return results;
        }

        /**
         * Method which utlises Wfuzz for fuzzing clips from a stream.
         * @param streamID Long value which represents the stream ID for which clips should be fuzzed for.
         * @param reps     Integer value which represents the maximum range for a particular stream.
         * @return List<string>    string List which holds all of the results of clips.
         */
        private static List<string> wfuzz(long streamID, int reps)
        {
            List<string> fuzzRes = new List<string>();
            string cmd = "wfuzz -o csv -z range,0-" + reps + " --hc 404 https://clips-media-assets2.twitch.tv/" + streamID + "-offset-FUZZ.mp4";
            string args = "wfuzz -o csv -z range,0-" + reps + " --hc 404 https://clips-media-assets2.twitch.tv/" + streamID + "-offset-FUZZ.mp4";
            try
            {
                // TODO : not sure what wfuzz app is, and how to exec it, lib ? java ? bash ?
                ProcessStartInfo pi = new ProcessStartInfo(cmd, args)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                };
                var proc = Process.Start(pi);
                var br = proc.StandardOutput;

                string line;
                bool atResults = false;
                Regex wp = new Regex("(\\d*),(\\d*),(\\d*,\\d*,\\d*),(\\d*),(\\d*)");
                double quarters = 0;
                int found = 0;
                while ((line = br.ReadLine()) != null)
                {
                    if (atResults)
                    {
                        var wm = wp.Matches(line);
                        if (wm.Count > 0)
                        {
                            if (int.Parse(wm[0].Groups[1].ToString()) % 900 == 0 && true)
                            {   //TODO: Fix the CLI bool usage.
                                quarters++;
                                if (found == 1)
                                {
                                    Console.WriteLine("\n" + (quarters / 4) + " hours into the VOD. " + found + " clip found so far. Continuing to find clips...");
                                }
                                else
                                {
                                    Console.WriteLine("\n" + (quarters / 4) + " hours into the VOD. " + found + " clips found so far. Continuing to find clips...");
                                }
                            }
                            if (wm[0].Groups[2].ToString().Equals("200"))
                            {
                                found++;
                                fuzzRes.Add("https://clips-media-assets2.twitch.tv/" + streamID + "-offset-" + wm[0].Groups[4] + ".mp4");
                            }
                        }
                    }
                    else if (line.IndexOf("id,") == 0)
                    {
                        atResults = true;
                    }
                }
            }
            catch (IOException)
            {
                fuzzRes.Add("Error using Wfuzz. Please make sure you have installed Wfuzz correctly and it is working.");
            }
            return fuzzRes;
        }

        /**
         * Method which utilises available fuzzing tools in JSE8 to fuzz for
         * clips from a given stream.
         * NOTICE: Extremely slow.
         * @param streamID Long value which represents the stream ID for which clips should be fuzzed for.
         * @param reps     Integer value which represents the maximum range for a particular stream.
         * @return List<string>    string List which holds all of the results of clips.
         */
        private static List<string> jFuzz(long streamID, int reps)
        {
            List<string> jfuzzRes = new List<string>();
            string baseURL = "https://clips-media-assets2.twitch.tv/" + streamID + "-offset-";
            for (int i = 0; i < reps; i++)
            {
                string clip = baseURL + i + ".mp4";
                try
                {
                    WebClient wc = new WebClient();
                    wc.OpenRead(clip);

                    jfuzzRes.Add(clip);
                }
                catch (IOException) { }
            }
            return jfuzzRes;
        }

        /**
         * This method gets all of the Twitch M3U8 VOD domains
         * from the domains file of the Twitch Recover repository.
         * @return List<string>    string List representing all of the Twitch M3U8 VOD domains.
         */
        private static List<string> getDomains()
        {
            List<string> domains = new List<string>();
            bool added = false;
            try
            {
                Uri dUrl = new Uri("https://raw.githubusercontent.com/esseivan/TwitchRecoverCS/main/domains.txt");

                HttpWebRequest request = WebRequest.CreateHttp(dUrl);
                request.UserAgent = "Mozilla/5.0";

                using (HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse())
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream stream = httpResponse.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string line = null;
                            while ((line = reader.ReadLine()) != null)
                            {
                                string response = line;
                                domains.Add(response);
                                added = true;
                            }
                        }
                    }
                }
            }
            catch (IOException) { }
            finally
            {
                if (!added)
                {     //To execute if the domains from the domains file were not added as a backup.
                    domains.Add("https://vod-secure.twitch.tv");
                    domains.Add("https://vod-metro.twitch.tv");
                    domains.Add("https://d2e2de1etea730.cloudfront.net");
                    domains.Add("https://dqrpb9wgowsf5.cloudfront.net");
                    domains.Add("https://ds0h3roq6wcgc.cloudfront.net");
                    domains.Add("https://dqrpb9wgowsf5.cloudfront.net");
                }
            }
            return domains;
        }

        /**
         * Checks if a URL is up by querying it
         * and checking if it returns a 200 response code.
         * @param url       URL to check.
         * @return bool  Boolean value which is true if querying the URL returns a
         * 200 response code or false if otherwise.
         */
        protected static bool checkURL(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(url);
                request.UserAgent = "Mozilla/5.0";

                using (HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse())
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /**
         * This method is responsible for brute forcing the
         * VOD URLs based on a timestamp that is correct
         * up to the minute.
         * @param name                  string value which represents the streamer's name.
         * @param streamID              Long value representing the stream ID.
         * @param timestamp             Long value representing the UNIX timestamp of the minute in question.
         * @return List<string>    string List which represents the working
         * VOD M3U8 URLs.
         */
        public static List<string> BFURLs(string name, long streamID, long timestamp)
        {
            List<string> results = new List<string>();
            for (int i = 0; i < 60; i++)
            {
                string url = Compute.URLCompute(name, streamID, timestamp + i);
                if (checkURL(url))
                {
                    List<string> vResults = verifyURL(url);
                    foreach (string u in vResults)
                    {
                        results.Add(u);
                    }
                }
            }
            return results;
        }

        /**
         * Checks each completed URL based on the given
         * URL value and the domains.
         * @param url                   string value representing the URL to verify.
         * @return List<string>    string List representing the
         * working VOD M3U8 URLs.
         */
        public static List<string> verifyURL(string url)
        {
            List<string> domains = getDomains();
            List<string> results = new List<string>();
            foreach (string d in domains)
            {
                if (checkURL(d + url))
                {
                    results.Add(d + url);
                }
            }
            return results;
        }

        /**
         * This method fuzzes all
         * of the possible qualities
         * for a specific URL.
         * @param part1     string value representing the part of the URL prior to the quality value.
         * @param part2     string value representing the part of the URL after the quality value.
         * @return Feeds    Feeds object containing the list of feeds found and their respective qualities.
         */
        public static Feeds fuzzQualities(string part1, string part2)
        {
            Feeds feeds = new Feeds();
            foreach (Quality qual in Quality.values().Values)
            {
                if (checkURL(part1 + qual.video + part2))
                {
                    feeds.addEntry(part1 + qual.video + part2, qual);
                }
            }
            return feeds;
        }
    }
}
