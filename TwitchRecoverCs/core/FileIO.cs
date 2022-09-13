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
using TwitchRecoverCs.core.Enums;

namespace TwitchRecoverCs.core
{
    /**
     * This class handles all IO related to files.
     */
    public class FileIO
    {
        public static string fn = "";     //string value which holds the latter part of the individual file name.

        /**
         * This method creates a file
         * and writes to it.
         * @param values    string arraylist representing the values to write to the file.
         * @param fp        string value representing the complete file path of the file to create and write to.
         */
        public static void write(List<string> values, string fp)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fp))
                {
                    string writeStr = string.Join("\n", values);
                    sw.Write(writeStr);
                }
            }
            catch (IOException) { }
        }

        /**
         * This method reads the contents of a file.
         * @param fp                    Complete filepath of the file to read.
         * @return List<string>    string arraylist representing all of the contents of the file.
         */
        public static List<string> read(string fp)
        {
            List<string> contents = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(fp))
                {
                    while (!sr.EndOfStream)
                    {
                        contents.Add(sr.ReadLine());
                    }
                }
            }
            catch (IOException) { }
            return contents;
        }

        /**
         * This method adjusts a user
         * inputted file path.
         * @param fp        File path inputted by the user, to be adjusted.
         * @return string   string value representing the adjusted file path.
         */
        public static string adjustFP(string fp)
        {
            if (fp.IndexOf('\\') != fp.Length - 1)
            {
                fp += "\\";
            }
            return fp;
        }

        /**
         * This method checks if a file
         * currently exists at the specific
         * location.
         * @param fp        string value representing the complete file path to check for (excluding file extension).
         * @param fe        FileExtensions enum which represents the anticipated file extension of the file.
         * @return boolean  Boolean value representing whether or not a file alredy exists at that location or not.
         */
        public static bool checkFileExistence(string fp, FileExtension fe)
        {
            return File.Exists(fp + fe);
        }

        /**
         * This method computes the file name for a
         * content to be downloaded from the given
         * ID and content type.
         * @param ct        Content type enum representing the content type of the content in question.
         * @param id        string value representing the ID (clip slug, VOD ID, etc.) of the content.
         * @return string   string value representing the compute file name (excluding file extension).
         */
        public static string computeFN(ContentType ct, string id)
        {
            return "TwitchRecover-" + ct.ToString() + "-" + id;
        }

        /**
         * This method exports the results of retrieved URLs.
         * @param results   string arraylist containing all of the results to be exported.
         * @param fp        string value representing the complete final file path of the output file.
         */
        public static void exportResults(List<string> results, string fp)
        {
            results.Insert(0, "# Results generated using Twitch Recover - https://github.com/twitchrecover/twitchrecover");
            results.Insert(1, "# Please consider donating if this has been useful for you - https://paypal.me/daylamtayari");
            write(results, fp);
        }

        /**
         * This method exports all
         * of the feeds of a feed
         * object to a file.
         * @param feeds     Feeds object to export.
         * @param fp        string value representing the complete file path of the file to output.
         */
        public static void exportFeeds(Feeds feeds, string fp)
        {
            List<string> results = new List<string>();
            for (int i = 0; i < feeds.getFeeds().Count; i++)
            {
                results.Add("# Quality: " + feeds.getQuality(i).text);
                results.Add(feeds.getFeed(i));
            }
            exportResults(results, fp);
        }

        /**
         * This method 'converts' a file
         * from one file extension to another.
         * Both old and new file extensions must
         * be compatible since all this does is
         * change the file extension.
         * @param fp        The current complete file path of the file to convert.
         * @param fe        FileExtension enum value representing the desired output file extension.
         * @return string   string value representing the absolute file path of the new file.
         */
        public static string convert(string fp, FileExtension fe)
        {
            string newFP = fp.Substring(0, fp.LastIndexOf(".")) + fe.fileExtension;
            File.Move(fp, newFP);
            return newFP;
        }

        /**
         * This method parses an arraylist of
         * values that are from a read file and
         * removes line which are deemed to be
         * comments.
         * @param read                  string arraylist containing the raw lines of values read from a file.
         * @return List<string>    string arraylist containing the read values excluding comment lines.
         */
        public static List<string> parseRead(List<string> read)
        {
            read.Where((line) => !(line.StartsWith("#") || string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("/**") || line.StartsWith("**/") || line.StartsWith("*")));
            return read;
        }
    }
}
