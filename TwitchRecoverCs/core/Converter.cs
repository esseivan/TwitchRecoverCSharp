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
     * Converter object class
     * which converts a TS file to
     * another file extension.
     */
    public class Converter
    {
        private string ogFP;            //string value representing the original file path.
        private FileExtension fe;       //FileExtension enum which represents the desired output file extension.
        private string rawFP;           //string value representing the raw file path without the current file extension.

        /**
         * Constructor for the Converter object.
         * @param ogFP  string value representing the complete original file path of the file to convert.
         * @param fe    FileExtension enum value representing the output desired file extension.
         */
        public Converter(string ogFP, FileExtension fe)
        {
            this.ogFP = ogFP;
            this.fe = fe;
        }

        /**
         * Main method for the Converter object class.
         */
        public void main()
        {
            if (!checkTS())
            {
                Console.WriteLine("ERROR: File is not a TS file.");
                return;
            }
            getRaw();
            File.Move(ogFP, rawFP + fe.fileExtension);
        }

        /**
         * This gets the raw file path
         * from the given file path.
         */
        private void getRaw()
        {
            rawFP = ogFP.Substring(0, ogFP.LastIndexOf('.'));
        }

        /**
         * This checks if the inputted file is
         * indeed a TS file.
         * @return Boolean  Boolean value representing whether or not the file is a TS file.
         */
        private bool checkTS()
        {
            if (ogFP.Substring(ogFP.LastIndexOf(".")).ToLower().Equals(".ts"))
            {
                return true;
            }
            return false;
        }
    }
}
