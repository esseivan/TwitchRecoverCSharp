﻿/*
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
using System.Threading;
using System.Threading.Tasks;
using TwitchRecoverCs.core.API;
using TwitchRecoverCs.core.Downloader;
using TwitchRecoverCs.core.Enums;

namespace TwitchRecoverCs.core
{
    /**
     * The VOD object holds
     * all of the elements and
     * methods necessary to process
     * a VOD.
     */
    public class VOD
    {
        private bool isDeleted;                  //Boolean value representing whether or not a VOD is still up.
        private Feeds feeds;                        //Feeds object corresponding to the VOD.
        private FileExtension fe;                   //Desired output file extension.
        private long VODID;                         //VOD ID of a VOD if it is still up.
        private string channelName;                 // Channel name, used only for save file default name
        private string[] vodInfo;                   //string array containing the VOD info such as streamer, timestamp, etc.
                                                    //0: Channel name; 1: Stream ID; 2. Timestamp of the start of the stream; 3: Brute force bool.
        private List<string> retrievedURLs;    //Arraylist containing all of the VOD 'chunked' M3U8s of a particular VOD.
        private string fp;                          //string value representing the file path of the output file.
        private string fn;                          //string value representing the file name of the output file.
        private string fFP;                         //string value which represents the final file path of the downloaded object.

        public string ChannelName { get => channelName; set => channelName = value; }
        public long ID { get => VODID; }

        /**
         * The constructor of a
         * VOD object which initialises
         * two bool values based on given inputs
         * and if necessary initialises the vodInfo
         * string array.
         * @param isDeleted     Boolean value representing whether or not the VOD has being deleted or not.
         */
        public VOD(bool isDeleted)
        {
            this.isDeleted = isDeleted;
            if (isDeleted)
            {
                vodInfo = new string[4];
            }
        }

        /**
         * This method processes the downloading of a
         * VOD.
         * @param fe    FileExtension enum representing the desired output file extension.
         * @param feed  string value representing the desired feed to download.
         */
        public async void downloadVOD(FileExtension fe, string feed, int minChunk, int maxChunk, CancellationToken token)
        {
            computeFN();
            //if (vodInfo == null)
            //{
            //    getVODFeeds();
            //}
            //else
            //{
            //    retrieveVOD(false);
            //    retrieveVODFeeds();
            //}
            fFP = fp + fn + fe.fileExtension;
            try
            {
                await Download.m3u8Download(feed, fFP, minChunk, maxChunk, token);
            }
            catch (IOException ex)
            {
                Console.WriteLine("FATAL : " + ex.ToString());
            }
        }

        /**
         * This method processes the downloading of a
         * VOD.
         * @param fe    FileExtension enum representing the desired output file extension.
         * @param minChunk  starting chunk. -1 for disbaled
         * @param maxChunk  ending chunk. -1 for disbaled
         */
        public async Task<string> downloadVOD(string feed, int minChunk, int maxChunk, CancellationToken token)
        {
            fFP = fp;
            string result = null;
            try
            {
                result = await Download.m3u8Download(feed, fFP, minChunk, maxChunk, token);
            }
            catch (IOException ex)
            {
                Console.WriteLine("FATAL : " + ex.ToString());
            }

            return result;
        }

        /**
         * This method exports the results
         * of the retrieved URLs.
         */
        public void exportResults()
        {
            computeFN();
            fFP = fp + fn + FileExtension.values()[FileExtensionTypes.TXT].fileExtension;
            FileIO.exportResults(retrievedURLs, fFP);
        }

        /**
         * This method exports the feeds
         * object of the object class.
         */
        public void exportFeed()
        {
            computeFN();
            fFP = fp + fn + FileExtension.values()[FileExtensionTypes.TXT].fileExtension;
            FileIO.exportFeeds(feeds, fFP);
        }

        /**
         * This method gets an arraylist
         * of chunked (source quality)
         * VOD feeds from given information.
         * @return List<string>    string arraylist containing all of the source VOD feeds.
         */
        public List<string> retrieveVOD(bool wr)
        {
            if (!wr)
            {
                retrievedURLs = VODRetrieval.retrieveVOD(vodInfo[0], vodInfo[1], vodInfo[2], false);
            }
            else
            {
                retrievedURLs = VODRetrieval.retrieveVOD(vodInfo[0], vodInfo[1], vodInfo[2], bool.Parse(vodInfo[3]));
            }
            return retrievedURLs;
        }

        /**
         * This method retrieves the list of
         * all possible feeds for a deleted VOD.
         * @return Feeds    Feeds object containing all possible feeds of a deleted VOD.
         */
        public Feeds retrieveVODFeeds()
        {
            if (retrievedURLs.Count == 0)
            {
                return null;
            }
            else
            {
                feeds = VODRetrieval.retrieveVODFeeds(retrievedURLs.ElementAt(0));
                return feeds;
            }
        }

        /**
         * This method uses a website analytics
         * link to get all the values of
         * the vodInfo array.
         * @param url   string value representing a website analytics URL.
         */
        public void retrieveVODURL(string url)
        {
            vodInfo = WebsiteRetrieval.getData(url);
        }

        /**
         * This method gets the corresponding
         * Feeds object to a given VOD ID.
         * @return Feeds    Feeds object corresponding to the VOD of the VOD ID.
         */
        public Feeds getVODFeeds()
        {
            feeds = VideoAPI.getVODFeeds(VODID);
            if (feeds.getFeeds().Count == 0)
            {
                feeds = VideoAPI.getSubVODFeeds(VODID, false);
            }
            return feeds;
        }

        /**
         * Accessor for a single particular
         * feed of the Feeds object based
         * on a given integer ID.
         * @param id        Integer value representing the list value of the feed to fetch.
         * @return string   Feed URL corresponding to the given ID.
         */
        public string getFeed(int id)
        {
            return feeds.getFeed(id);
        }

        /**
         * Accessor for the Feeds
         * object of the VOD object.
         * @return Feeds    Feeds object of the VOD object.
         */
        public Feeds getFeeds()
        {
            return feeds;
        }

        /**
         * Accessor for the fFP variable.
         * @return string   string value representing the final file path of the outputted object.
         */
        public string getFFP()
        {
            return fFP;
        }

        /**
         * Accessor for the retrievedURLs
         * arraylist.
         * @return List<string>    The retrievedURLs string arraylist.
         */
        public List<string> getRetrievedURLs()
        {
            return retrievedURLs;
        }

        /**
         * Mutator for the
         * VODID variable.
         * @param VODID     Long value which represents the VODID of the VOD.
         */
        public void setID(long VODID)
        {
            this.VODID = VODID;
        }

        /**
         * Mutator for the file extension
         * enum which represents the user's
         * desired file output format.
         * @param fe    A FileExtensions enum which represents the user's desired output file extension.
         */
        public void setFE(FileExtension fe)
        {
            this.fe = fe;
        }

        /**
         * Mutator for the VOD info
         * string array which contains
         * all of the information about a
         * VOD in order to compute the base URL.
         * @param info      string array containing the information about the VOD.
         */
        public void setVODInfo(string[] info)
        {
            vodInfo = info;
        }

        /**
         * Mutator for the channel name
         * value of the vodInfo array.
         * @param channel   string value representing the channel the VOD is from.
         */
        public void setChannel(string channel)
        {
            vodInfo[0] = channel;
        }

        /**
         * Mutator for the stream ID
         * value of the vodInfo array.
         * @param streamID  string value representing the stream ID of the stream of the VOD.
         */
        public void setStreamID(string streamID)
        {
            vodInfo[1] = streamID;
        }

        /**
         * Mutator for the timestamp
         * value of the vodInfo array.
         * @param timestamp     string value representing the timestamp of the start of the VOD in 'YYYY-MM-DD HH:mm:ss' format.
         */
        public void setTimestamp(string timestamp)
        {
            vodInfo[2] = timestamp;
        }

        /**
         * Mutator for the brute force
         * value of the vodInfo array.
         * @param bf    Boolean value representing whether or not the VOD start timestamp is to the second or to the minute.
         */
        public void setBF(bool bf)
        {
            vodInfo[3] = bf.ToString();
        }

        /**
         * This method sets the file path
         * by first adjusting the user
         * inputted file path.
         * @param fp    User inputted file path.
         */
        public void setFP(string fp)
        {
            this.fp = fp;//FileIO.adjustFP(fp);
        }

        public void retrieveID(string url)
        {
            VODID = VODRetrieval.retrieveID(url);
            channelName = VODRetrieval.retrieveChannel(url);
        }

        /**
         * This method computes the file name
         * based on what info was provided.
         */
        private void computeFN()
        {
            if (vodInfo == null)
            {
                fn = FileIO.computeFN("VOD", VODID.ToString());
            }
            else
            {
                fn = FileIO.computeFN("VOD", vodInfo[1]);
            }
        }
    }
}
