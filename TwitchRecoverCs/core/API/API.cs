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
using System.Net.Cache;
using System.Net.Configuration;
using System.Net.Http;
using System.Net.Mime;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchRecoverCs.core.Downloader;
using TwitchRecoverCs.core.Enums;

namespace TwitchRecoverCs.core.API
{
    /**
     * This class handles all of the
     * API calls and retrievals.
     */
    public class API
    {
        /**
     * Method which parses the feeds from a given
     * arraylist which includes all of the lines that
     * were read from the web query and creates and
     * returns a Feeds object which contains all of
     * the feeds and their corresponding qualities.
     * @param response  string arraylist which contains all of the lines read in the web query.
     * @return  Feeds   A Feeds object which contains all of the feed URLs and their corresponding qualities.
     */
        public static Feeds parseFeeds(List<string> response)
        {
            Feeds feeds = new Feeds();
            for (int i = 0; i < response.Count; i++)
            {
                if (!response.ElementAt(i).StartsWith("#"))
                {
                    if (response.ElementAt(i - 2).Contains("chunked")) //For when the line is the source feed.
                    {
                        feeds.addEntryPos(response.ElementAt(i), Quality.values()[QualityTypes.Source], 0);
                        if (response.ElementAt(i - 2).Contains("Source"))
                        {
                            feeds.addEntryPos(response.ElementAt(i), Quality.getQualityRF(Compute.singleRegex("#EXT-X-STREAM-INF:BANDWIDTH=\\d*,CODECS=\"[a-zA-Z0-9.]*,[a-zA-Z0-9.]*\",RESOLUTION=(\\d*x\\d*),VIDEO=\"chunked\"", response.ElementAt(i - 1)), 60.000), 1);
                        }
                        else
                        {
                            //Get the FPS of the source resolution.
                            string patternF = "#EXT-X-MEDIA:TYPE=VIDEO,GROUP-ID=\"chunked\",NAME=\"([0-9p]*) \\(source\\)\",AUTOSELECT=[\"YES\"||\"NO\"]*,DEFAULT=[\"YES\"||\"NO\"]*";
                            Regex pF = new Regex(patternF);
                            MatchCollection mF = pF.Matches(response.ElementAt(i - 2));

                            double fps = 0.000;
                            if (mF.Count > 0)
                            {
                                string vid = mF[0].Groups[1].ToString();
                                // Handle resolutions like 720p w/o fps attribute like 720p30fps
                                if (vid.IndexOf('p') == vid.Length - 1)
                                    fps = 60.00;
                                else
                                    fps = double.Parse(vid.Substring(vid.IndexOf('p') + 1));
                            }
                            //Get the resolution of the source resolution.
                            string pattern = "#EXT-X-STREAM-INF:BANDWIDTH=\\d*,RESOLUTION=(\\d*x\\d*),CODECS=\"[a-zA-Z0-9.]*,[a-zA-Z0-9.]*\",VIDEO=\"chunked\"";
                            Regex p = new Regex(pattern);
                            MatchCollection m = p.Matches(response.ElementAt(i - 1));

                            if (m.Count > 0)
                            {
                                feeds.addEntryPos(response.ElementAt(i), Quality.getQualityRF(m[0].Groups[1].ToString(), fps), 1);
                            }
                        }
                    }
                    else if (response.ElementAt(i - 2).Contains("audio"))
                    {       //For when the line is an audio-only feed.
                        feeds.addEntry(response.ElementAt(i), Quality.values()[QualityTypes.AUDIO]);
                    }
                    else if (response.ElementAt(i - 2).Contains("1080p60"))
                    {     //For resolutions greater or equal to 1080p60.
                          //Get the FPS of the source resolution.
                        string patternF = "#EXT-X-MEDIA:TYPE=VIDEO,GROUP-ID=\"1080p[0-9]*\",NAME=\"(1080p[0-9]*)\",AUTOSELECT=[\"YES\"||\"NO\"]*,DEFAULT=[\"YES\"||\"NO\"]*";
                        Regex pF = new Regex(patternF);
                        MatchCollection mF = pF.Matches(response.ElementAt(i - 2));
                        double fps = 0.000;
                        if (mF.Count > 0)
                        {
                            string vid = mF[0].Groups[1].ToString();
                            fps = double.Parse(vid.Substring(vid.IndexOf('p') + 1));
                        }
                        //Get the resolution of the source resolution.
                        string pattern = "#EXT-X-STREAM-INF:BANDWIDTH=\\d*,CODECS=\"[a-zA-Z0-9.]*,[a-zA-Z0-9.]*\",RESOLUTION=(\\d*x\\d*),VIDEO=\"1080p[0-9]*\"";
                        Regex p = new Regex(pattern);
                        MatchCollection m = pF.Matches(response.ElementAt(i - 1));
                        if (m.Count > 0)
                        {
                            feeds.addEntry(response.ElementAt(i), Quality.getQualityRF(m[0].Groups[1].ToString(), fps));
                        }
                    }
                    else
                    {
                        feeds.addEntry(response.ElementAt(i), Quality.getQualityV(Compute.singleRegex("#EXT-X-MEDIA:TYPE=VIDEO,GROUP-ID=\"([\\d]*p[36]0)\",NAME=\"([0-9p]*)\",AUTOSELECT=[\"YES\"||\"NO\"]*,DEFAULT=[\"YES\"||\"NO\"]*", response.ElementAt(i - 2))));
                    }
                }
            }
            return feeds;
        }

        /**
         * This method performs a get request of a
         * specific given URL (which has to be a
         * Twitch API URL that is setup in at least
         * V5 on the backend.
         * @param url                   string value representing the URL to perform the get request on.
         * @return List<string>    string arraylist holding the entire response from the get request, each line representing an entry.
         */
        public static List<string> getReq(string url)
        {
            List<string> responseContents = new List<string>();
            try
            {
                HttpWebRequest httpget = WebRequest.CreateHttp(url);
                //request.AutomaticDecompression = DecompressionMethods.GZip; TODO : check if required
                httpget.Headers.Add("User-Agent", "Mozilla/5.0");
                httpget.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
                httpget.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");   //Web client client ID (check out my explanation of Twitch's video system for more details).
                using (HttpWebResponse httpResponse = (HttpWebResponse)httpget.GetResponse())
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK /*200*/)
                    {
                        using (Stream stream = httpResponse.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                responseContents.Add(line);
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return responseContents;
        }

        /**
         * This method gets a playlist
         * file and returns the contents
         * of the playlist file.
         * @param url       string value which represents the URL of the playlist file.
         * @return Feeds    Feeds object containing all of the feeds from an M3U8 playlist.
         */
        public static Feeds getPlaylist(string url, string downloadedFile)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    //wc.DownloadFile(url, downloadedFile);

                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    var response = ex.Response;
                    var dataStream = response.GetResponseStream();
                    var reader = new StreamReader(dataStream);
                    var details = reader.ReadToEnd();
                }
            }

            if (downloadedFile == null)
                return null;
            return parseFeeds(File.ReadAllLines(downloadedFile).ToList());
        }

        /**
         * This method parses and returns
         * the token and signature values
         * from a given API JSON response.
         * @param response      string containing the JSON response from the API call.
         * @param isVOD         Boolean value that is true if the token is for a video and false if it is for a stream.
         * @return string[]     string array containing the token and signature values.
         * string[2]: 0: Token; 1: Signature.
         */
        protected static string[] parseToken(string response, bool isVOD)
        {
            string[] results = new string[2];
            //Parse JSON:
            JsonDocument jO = JsonDocument.Parse(response);
            var tokenCat = jO.RootElement.GetProperty("data");
            if (isVOD)
                tokenCat = tokenCat.GetProperty("videoPlaybackAccessToken");
            else
                tokenCat = tokenCat.GetProperty("streamPlaybackAccessToken");

            string token = tokenCat.GetProperty("value").GetString();
            results[1] = tokenCat.GetProperty("signature").GetString();
            results[0] = token.Replace("\\", ""); //Remove back slashes from token

            return results;
        }

        /**
         * This method retrieves an
         * @param id            string value representing the ID to input into the query.
         * @param isVOD         Boolean value that is true if the token being retrieved is for a VOD and false if it is for a live stream.
         * @return string[]     string array containing the token and signature values.
         * string[2]: 0: Token; 1: Signature.
         */
        public static string[] getToken(string id, bool isVOD)
        {
            string json;
            string response = "";
            if (isVOD)
            {
                json = "{\"operationName\": \"PlaybackAccessToken\",\"variables\": {\"isLive\": false,\"login\": \"\",\"isVod\": true,\"vodID\": \"" + id + "\",\"playerType\": \"channel_home_live\"},\"extensions\": {\"persistedQuery\": {\"version\": 1,\"sha256Hash\": \"0828119ded1c13477966434e15800ff57ddacf13ba1911c129dc2200705b0712\"}}}";
            }
            else
            {
                json = "{\"operationName\": \"PlaybackAccessToken\",\"variables\": {\"isLive\": true,\"login\": \"" + id + "\",\"isVod\": false,\"vodID\": \"\",\"playerType\": \"channel_home_live\"},\"extensions\": {\"persistedQuery\": {\"version\": 1,\"sha256Hash\": \"0828119ded1c13477966434e15800ff57ddacf13ba1911c129dc2200705b0712\"}}}";
            }
            try
            {
                HttpWebRequest httppost = WebRequest.CreateHttp("https://gql.twitch.tv/gql");
                //request.AutomaticDecompression = DecompressionMethods.GZip; TODO : check if required
                httppost.Method = "POST";
                httppost.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
                httppost.ContentType = "text/plain;charset=UTF-8";

                using (Stream requestBody = httppost.GetRequestStream())
                {
                    var bytesArr = Encoding.UTF8.GetBytes(json);
                    requestBody.Write(bytesArr, 0, bytesArr.Length);
                }

                using (HttpWebResponse httpResponse = (HttpWebResponse)httppost.GetResponse())
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK /*200*/)
                    {
                        using (Stream stream = httpResponse.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            response = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception) { }

            return parseToken(response, isVOD);
        }
    }
}
