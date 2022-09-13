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
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TwitchRecoverCs.core
{
    /**
     * This class Contains the core method for website data
     * recovery and all of its necessary methods to retrieve
     * necessary information from the Twitch analytics websites
     * Twitch recover supports.
     */
    public class WebsiteRetrieval
    {
        //Core methods:

        /**
         * Core method which retrieves the 4 principal values (streamer's name, stream ID, timestamp, duration)
         * of a stream and returns in a string array in that order.
         * @param url URL to retrieve the values from.
         * @return string[4]    string array containing the 4 principal values (streamer's name, stream ID,
         * timestamp of the start of the stream and the duration) in that respective order. If all values of the
         * array are null, the URL is invalid.
         */
        public static string[] getData(string url)
        {
            string[] results = new string[4];     //0: streamer's name; 1: Stream ID; 2: Timestamp; 3: Duration.
            int source = checkURL(url);
            if (source == -1)
            {         //Invalid URL.
                return results;
            }
            else if (source == 1)
            {     //Twitch Tracker URL.
                try
                {
                    results = getTTData(url);
                }
                catch (IOException) { }
                return results;
            }
            else if (source == 2)
            {     //Stream Charts URL.
                try
                {
                    results = getSCData(url);
                }
                catch (IOException) { }
                return results;
            }
            return results;
        }

        /**
         * This method checks if a URL is a stream URL
         * from one of the supported analytics websites.
         * @param url URL to be checked.
         * @return int      Integer that is either -1 if the URL is invalid or
         * a value that represents which analytics service the stream link is from.
         */
        private static int checkURL(string url)
        {
            if (url.Contains("twitchtracker.com/") && url.Contains("/streams/"))
            {
                return 1;   //Twitch Tracker URL.
            }
            else if (url.Contains("streamscharts.com/twitch/channels/") && url.Contains("/streams/"))
            {
                return 2;   //Streams Charts URL.
            }
            return -1;
        }

        /**
         * This method gets the JSON return from a URL.
         * @param url string representing the URL to get the JSON response from.
         * @return string   string response representing the JSON response of the URL.
         * @throws IOException
         */
        private static string getJSON(string url)
        {
            string json = "";
            Uri jsonFetch = new Uri(url);
            HttpWebRequest httpcon = WebRequest.CreateHttp(jsonFetch);
            httpcon.Headers.Add("User-Agent", "Mozilla/5.0");

            string readLine = null;
            HttpWebResponse httpResponse = (HttpWebResponse)httpcon.GetResponse();
            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                using (Stream stream = httpResponse.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    StringBuilder response = new StringBuilder();

                    while ((readLine = reader.ReadLine()) != null)
                    {
                        response.Append(readLine);
                    }

                    json = response.ToString();
                }
            }
            return json;
        }

        //Individual website retrieval:

        //Twitch Tracker retrieval:
        /**
         * This method gets the 4 principal values (streamer's name, stream ID, timestamp and the duration)
         * from a Twitch Tracker stream URL.
         * @param url string value representing the Twitch Tracker stream URL.
         * @return string[4]    string array containing the 4 principal values (streamer's name, stream ID,
         * timestamp of the start of the stream and the duration) in that respective order.
         * @throws IOException
         */
        private static string[] getTTData(string url)
        {
            string[]
            results = new string[4];
            Uri jsonFetch = new Uri(url);
            HttpWebRequest httpcon = WebRequest.CreateHttp(jsonFetch);
            httpcon.Headers.Add("User-Agent", "Mozilla/5.0");

            HttpWebResponse httpResponse = (HttpWebResponse)httpcon.GetResponse();
            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                using (Stream stream = httpResponse.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    //Get the timestamp:
                    string response;
                    string responseD = "";
                    for (int i = 0; i < 300; i++)
                    {
                        response = reader.ReadLine();
                        if (i == 7)
                        {
                            int tsIndex = response.IndexOf(" on ") + 4;
                            results[2] = response.Substring(tsIndex, tsIndex + 19);
                        }
                        //Stream duration fetcher:
                        if (response.Contains("stats-value to-time-lg"))
                        {
                            responseD = response;
                        }
                    }

                    //Get the stream duration:
                    string durationPattern = "<div class=\"stats-value to-time-lg\">(\\d*)</div>";
                    Regex dr = new Regex(durationPattern);
                    Match dm = dr.Match(responseD);
                    if (dm.Success)
                    {
                        results[3] = dm.Groups[1].ToString();
                    }
                    //Get the streamer's name and the VOD ID:
                    string pattern = "twitchtracker\\.com\\/([a-zA-Z0-9-_]*)\\/streams\\/(\\d*)";
                    Regex r = new Regex(pattern);
                    Match m = r.Match(url);
                    if (m.Success)
                    {
                        results[0] = m.Groups[1].ToString();
                        results[1] = m.Groups[2].ToString();
                    }
                    //Return the array:
                    return results;
                }
            }
            throw new IOException();
        }

        //Stream Charts retrieval:
        /**
         * This method gets the 4 principal values (streamer's name, stream ID, timestamp and the duration)
         * from a Stream Charts stream URL.
         * @param url string value representing the Stream Charts stream URL.
         * @return string[4]    string array containing the 4 principal values (streamer's name, stream ID,
         * timestamp of the start of the stream and the duration) in that respective order.
         * @throws IOException
         */
        private static string[] getSCData(string url)
        {
            string[]
            results = new string[4];     //0: streamer's name; 1: Stream ID; 2: Timestamp; 3: Duration.
            string userID;
            double duration = 0.0;
            //Retrieve initial values:
            string pattern = "streamscharts\\.com\\/twitch\\/channels\\/([a-zA-Z0-9_-]*)\\/streams\\/(\\d*)";
            Regex r = new Regex(pattern);
            Match m = r.Match(url);
            if (m.Success)
            {
                results[0] = m.Groups[1].ToString();
                results[1] = m.Groups[2].ToString();
            }

            //Retrieve user ID:
            string idJSON = getJSON("https://api.twitch.tv/v5/users/?login=" + results[0] + "&client_id=ohroxg880bxrq1izlrinohrz3k4vy6");
            JsonDocument joID = JsonDocument.Parse(idJSON);
            JsonElement.ArrayEnumerator users = joID.RootElement.GetProperty("users").EnumerateArray();
            var user = users.ElementAt(0);

            userID = user.GetProperty("_id").GetString();

            //Retrieve stream values:
            string dataJSON = getJSON("https://alla.streamscharts.com/api/free/streaming/platforms/1/channels/" + userID + "/streams/" + results[1] + "/statuses");

            JsonDocument joD = JsonDocument.Parse(dataJSON);
            JsonElement.ArrayEnumerator items = joD.RootElement.GetProperty("items").EnumerateArray();
            for (int i = 0; i < items.Count(); i++)
            {
                var item = items.ElementAt(i);
                if (i == 0)
                {
                    results[2] = item.GetProperty("stream_created_at").GetString();
                }
                duration += item.GetProperty("air_time").GetDouble();
            }
            results[3] = (duration * 60).ToString();
            return results;
        }
    }
}
