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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwitchRecoverCs.core.Downloader;
using TwitchRecoverCs.core.Enums;

namespace TwitchRecoverCs.core.API
{
    /**
     * This class handles all
     * of the API methods directly
     * related to VODs.
     */
    public class VideoAPI
    {
        /**
         * This method gets the list of feeds
         * of a VOD that is still up from the
         * VOD ID.
         * This is NOT to be used for sub-only VODs.
         * @param VODID     Long value representing the VOD ID.
         * @return Feeds    Feeds object holding the list of VOD feeds and their corresponding qualities.
         */
        public static Feeds getVODFeeds(long VODID)
        {
            FileDestroyer downloadedFile = FileHandler.createTempFile("TwitchRecover-Playlist-", ".m3u8");

            string[] auth = getVODToken(VODID);  //0: Token; 1: Signature.

            var res = API.getPlaylist(string.Format("https://usher.ttvnw.net/vod/{0}.m3u8?sig={1}&token={2}&allow_source=true&player=twitchweb&allow_spectre=true&allow_audio_only=true", VODID, auth[1], auth[0]), downloadedFile);

            return res;
        }

        /**
         * This method retrieves the M3U8 feeds for
         * sub-only VODs by utilising values provided
         * in the public VOD metadata API.
         * @param VODID     Long value representing the VOD ID to retrieve the feeds for.
         * @return Feeds    Feeds object holding all of the feed URLs and their respective qualities.
         */
        public static Feeds getSubVODFeeds(long VODID, bool highlight)
        {
            Feeds feeds = new Feeds();
            //Get the JSON response of the VOD:
            string response = "";
            try
            {
                HttpWebRequest httpget = WebRequest.CreateHttp(string.Format("https://api.twitch.tv/kraken/videos/{0}", VODID));
                httpget.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
                httpget.UserAgent = "Mozilla/5.0";
                httpget.Accept = "application/vnd.twitchtv.v5+json";

                using (HttpWebResponse httpResponse = (HttpWebResponse)httpget.GetResponse())
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

            //Parse the JSON response:
            JObject jO = JObject.Parse(response);
            string baseURL = Compute.singleRegex("https:\\/\\/[a-z0-9]*.cloudfront.net\\/([a-z0-9_]*)\\/storyboards\\/[0-9]*-info.json", jO["seek_previews_url"].ToString());

            string token = getVODToken(VODID)[0];

            JObject jO2 = JObject.Parse(token);
            var restricted = jO2["chansub"]["restricted_bitrates"].ToArray();

            if (highlight)
            {
                string domain = Compute.singleRegex("(https:\\/\\/[a-z0-9\\-]*.[a-z_]*.[net||com||tv]*\\/[a-z0-9_]*\\/)chunked\\/highlight-[0-9]*.m3u8", Fuzz.verifyURL("/" + baseURL + "/chunked/highlight-" + VODID + ".m3u8").ElementAt(0));
                for (int i = 0; i < restricted.Count(); i++)
                {
                    feeds.addEntry(domain + restricted.ElementAt(i).ToString() + "/highlight-" + VODID + FileExtension.values()[FileExtensionTypes.M3U8].fileExtension, Quality.getQualityV(restricted.ElementAt(i).ToString()));
                }
            }
            else
            {
                string domain = Compute.singleRegex("(https:\\/\\/[a-z0-9\\-]*.[a-z_]*.[net||com||tv]*\\/[a-z0-9_]*\\/)chunked\\/index-dvr.m3u8", Fuzz.verifyURL("/" + baseURL + "/chunked/index-dvr.m3u8").ElementAt(0));
                for (int i = 0; i < restricted.Count(); i++)
                {
                    feeds.addEntry(domain + restricted.ElementAt(i).ToString() + "/index-dvr" + FileExtension.values()[FileExtensionTypes.M3U8].fileExtension, Quality.getQualityV(restricted.ElementAt(i).ToString()));
                }
            }
            return feeds;
        }

        /**
         * This method retrieves the
         * token and signature values
         * for a VOD.
         * @param VODID     Long value representing the VOD ID to get the token and signature for.
         * @return String[] String array holding the token in the first position and the signature in the second position.
         * String[2]: 0: Token; 1: Signature.
         */
        private static string[] getVODToken(long VODID)
        {
            return API.getToken(VODID.ToString(), true);
        }
    }
}
