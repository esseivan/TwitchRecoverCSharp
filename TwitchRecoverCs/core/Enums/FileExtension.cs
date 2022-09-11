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
using static System.Net.Mime.MediaTypeNames;

namespace TwitchRecoverCs.core.Enums
{
    public enum FileExtensionTypes
    {
        M3U8,
        TS,
        MPEG,
        TXT,
        MOV,
        AVI,
        MP4,
    }

    /**
     * File extension enum which represents all
     * of the file extensions that are handled.
     */
    public class FileExtension
    {
        private static Dictionary<FileExtensionTypes, FileExtension> valuesList = null;
        public static Dictionary<FileExtensionTypes, FileExtension> values()
        {
            if (valuesList == null)
            {
                valuesList = new Dictionary<FileExtensionTypes, FileExtension>()
                {
                    { FileExtensionTypes.M3U8,  new FileExtension(".m3u8")   },
                    { FileExtensionTypes.TS,    new FileExtension(".ts")   },
                    { FileExtensionTypes.MPEG,  new FileExtension(".mpeg")   },
                    { FileExtensionTypes.TXT,   new FileExtension(".txt")   },
                    { FileExtensionTypes.MOV,   new FileExtension(".mov")   },
                    { FileExtensionTypes.AVI,   new FileExtension(".avi")   },
                    { FileExtensionTypes.MP4,   new FileExtension(".mp4")   },
                };
            }

            return valuesList;
        }

        public string fileExtension;
        FileExtension(string fe)
        {
            fileExtension = fe;
        }
    }
}
