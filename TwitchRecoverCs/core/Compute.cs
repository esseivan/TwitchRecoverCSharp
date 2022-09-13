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
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TwitchRecoverCs.core
{
    /**
     * This class contains the fundamental methods of the core package
     * and the ones that compute the fundamental elements of the
     * core of Twitch Recover.
     */
    public class Compute
    {
        /**
         * Method which
         * computes a VOD URL from given values.
         * @param name          String value representing the streamer's name.
         * @param streamID      A long representing the stream ID of a stream.
         * @param timestamp     A long value representing the timestamp of the stream
         * in standard timestamp form.
         * @return String       String value representing the completed latter part of the URL.
         */
        public static string URLCompute(string name, long streamID, long timestamp)
        {
            var baseString = name + "_" + streamID.ToString() + "_" + timestamp.ToString();
            var hash = Compute.hash(baseString);
            var finalString = hash + "_" + baseString;
            return "/" + finalString + "/chunked/index-dvr.m3u8";
        }

        /**
         * This method gets the UNIX time from a time value in a standard
         * timestamp format.
         * @param ts        String value representing the timestamp.
         * @return long     Long value which represents the UNIX timestamp.
         */
        public static long getUNIX(string ts)
        {
            var time = ts + " UTC";
            string DateTimeFormat = "yyyy-MM-dd HH:mm:ss zzz";
            if (DateTime.TryParseExact(time, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime df))
            {
                return ((DateTimeOffset)df).ToUnixTimeSeconds();
            }

            return 0;
        }

        /**
         * This method computers the SHA1 hash of the base
         * string computed in the URL compute method and
         * returns the first 20 characters of the hash.
         * @param baseString    Base string for which to compute the hash for.
         * @return String       First 20 characters of the SHA1 hash of the given base string.
         * @throws NoSuchAlgorithmException
         */
        private static string hash(string baseString)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(baseString));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString().Substring(0, 20);
            }
        }

        /**
         * This method computes the regex of a
         * given value and returns the value of
         * the first group, or if the pattern
         * did not match the given value, it
         * returns null.
         * @param pattern   String value representing the regex pattern to compile.
         * @param value     String value representing the value to apply the regex pattern to.
         * @return String   String value representing the first regex group or null if the regex did not compile.
         */
        public static string singleRegex(string pattern, string value)
        {
            Regex p = new Regex(pattern);
            var m = p.Match(value);
            if (m.Success)
            {
                return m.Groups[1].ToString();
            }
            return null;
        }

        /**
         * This method checks whether a string
         * has a null value or not.
         * @param string    String value to check.
         * @return boolean  Boolean value which is true if the string is null and false otherwise.
         */
        public static bool checkNullString(string @string)
        {
            if (@string == null)
            {
                return true;
            }
            return false;
        }
    }
}