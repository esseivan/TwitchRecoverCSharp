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
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TwitchRecoverCs.core.Enums;

namespace TwitchRecoverCs.core.API
{
    /**
     * This class handles all
     * of the API methods directly
     * related to clips.
     */
    public class ClipsAPI
    {
        /**
     * This method returns the
     * permanent clip link of a
     * clip from a given slug.
     * @param slug      string value representing the clip's slug.
     * @return string   string value representing the permanent link of the clip.
     */
        public static string getClipLink(string slug, bool download)
        {
            string response = "";
            //API Query:
            try
            {
                HttpWebRequest httpget = WebRequest.CreateHttp(string.Format("https://api.twitch.tv/kraken/clips/{0}", slug));
                httpget.Headers.Add("User-Agent", "Mozilla/5.0");
                httpget.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
                httpget.Headers.Add("Client-ID", "ohroxg880bxrq1izlrinohrz3k4vy6");

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
            try
            {
                JsonDocument jO = JsonDocument.Parse(response);
                string streamID = jO.RootElement.GetProperty("broadcast_id").GetString();
                JsonElement vod = jO.RootElement.GetProperty("vod");

                if (download)
                {
                    string url = vod.GetProperty("preview_image_url").GetString();
                    return url.Substring(0, url.IndexOf("-preview")) + FileExtension.values()[FileExtensionTypes.MP4].fileExtension;
                }
                else
                {
                    int offset = vod.GetProperty("offset").GetInt32() + 26;
                    return "https://clips-media-assets2.twitch.tv/" + streamID + "-offset-" + offset + FileExtension.values()[FileExtensionTypes.MP4].fileExtension;
                }
            }
            catch (Exception)
            {
                return "ERROR: Clip could not be found.";
            }
        }
    }
}
