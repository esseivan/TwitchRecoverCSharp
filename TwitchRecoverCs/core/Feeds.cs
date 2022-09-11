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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchRecoverCs.core.Enums;

namespace TwitchRecoverCs.core
{
    /**
     * Feeds object which holds a list
     * of feeds and their corresponding qualities.
     */
    public class Feeds
    {
        private List<string> feeds;        //String List which holds the links of all of the feeds.
        private List<Quality> qualities;   //Quality List which holds the corresponding qualities.
        //The feeds and qualities arraylist are corresponding,
        // the value at index 1 of one corresponds to the value at index 1 of the other.

        /**
         * Constructor of the feeds object
         * which initialises the feeds and
         * qualities Lists.
         */
        public Feeds()
        {
            feeds = new List<string>();
            qualities = new List<Quality>();
        }

        /**
         * Mutator for the feeds List which
         * sets the feeds List to the
         * given List.
         * @param f     String List which represents what the feeds List must be set to.
         */
        public void setFeeds(List<string> f)
        {
            feeds = f;
        }

        /**
         * Mutator for the qualities List which
         * sets the qualities List to the
         * given List.
         * @param q     Quality List which represents what the qualities List must be set to.
         */
        public void setQualities(List<Quality> q)
        {
            qualities = q;
        }

        /**
         * Mutator for both the feeds and qualities Lists,
         * adding an entry to both Lists.
         * @param f     A string value which represents a feed URL to be added to the feeds list.
         * @param q     A quality enum which represents a quality to be added to the qualities list.
         */
        public void addEntry(string f, Quality q)
        {
            feeds.Add(f);
            qualities.Add(q);
        }

        /**
         * Mutator for both the feeds and qualities Lists,
         * adding an entry to both Lists at one particular index.
         * @param f     A string value which represents a feed URL to be added to the feeds list.
         * @param q     A quality enum which represents a quality to be added to the qualities list.
         * @param i     An integer value representing the index at where to place the entry.
         */
        public void addEntryPos(string f, Quality q, int i)
        {
            feeds.Insert(i, f);
            qualities.Insert(i, q);
        }

        /**
         * Mutator for the feeds List whcih adds
         * the given string value to the feeds List.
         * @param f     A string value which represents a feed URL to be added to the feeds list.
         */
        public void addFeed(string f)
        {
            feeds.Add(f);
        }

        /**
         * Mutator for the qualities List which
         * adds the given Quality enum to the qualities List.
         * @param q     A quality enum which represents a quality to be added to the qualities list.
         */
        public void addQuality(Quality q)
        {
            qualities.Add(q);
        }

        /**
         * An accessor for the feeds List
         * which returns the URL of a feed at a
         * particular index of the feeds list.
         * @param i         Integer value representing the index of the feed to fetch.
         * @return String   String value representing the feed URL located at the given index.
         */
        public string getFeed(int i)
        {
            return feeds.ElementAt(i);
        }

        /**
         * An accessor that returns the feed url
         * that corresponds to a particular
         * given quality.
         * @param q         Quality enum for which to ElementAt the corresponding feed url.
         * @return String   String value representing the feed url that corresponds to
         * the given quality or is null if the quality enum does not exist in the Feeds object.
         */
        public string getFeedQual(Quality q)
        {
            for (int i = 0; i < qualities.Count; i++)
            {
                if (qualities.ElementAt(i) == q)
                {
                    return feeds.ElementAt(i);
                }
            }
            return null;     //If this point it reaches it means the quality wasn't present in the qualities list.
        }

        /**
         * An accessor for the qualiies
         * List which returns the Quality
         * enum located at a particular enum.
         * @param i         Integer value representing the index of the quality enum to fetch.
         * @return Quality  Quality enum representing the quality of the corresponding feed located at the given index.
         */
        public Quality getQuality(int i)
        {
            return qualities.ElementAt(i);
        }

        /**
         * An accessor that returns the quality
         * enum that corresponds to the quality
         * of a given feed URL.
         * @param feed      String value representing the feed value to find the corresponding quality of.
         * @return Quality  Quality enum which corresponds to the given feed URL
         * or is null if the feed URL does not exist in this Feeds object.
         */
        public Quality getQualityFeed(string feed)
        {
            for (int i = 0; i < feeds.Count; i++)
            {
                if (feeds.ElementAt(i).Equals(feed))
                {
                    return qualities.ElementAt(i);
                }
            }
            return null;    //If it reaches this point, the feed url does not exist in this feed object.
        }
        /**
         * An accessor for the feeds List
         * which returns the entire list of feeds.
         * @return List<String>    String List representing the entire feeds List.
         */
        public List<string> getFeeds()
        {
            return feeds;
        }

        /**
         * An accessor for the qualities List
         * which returns the entire list of qualities.
         * @return List<Quality>   Quality List which represents the entire qualities List.
         */
        public List<Quality> getQualities()
        {
            return qualities;
        }
    }
}
