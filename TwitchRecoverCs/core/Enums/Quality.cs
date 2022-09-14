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
using System.Security.Cryptography;


namespace TwitchRecoverCs.core.Enums
{
    /**
     * This enum represents the
     * different video quality options.
     */
    public enum QualityTypes
    {
        Source,
        QUADK60,
        QUADK,
        QHD4k60,
        QHD4k,
        QHD60,
        QHD,
        FHD60,
        FHD,
        FMHD60,
        FMHD,
        MHD60,
        MHD,
        HD60,
        HD,
        SHD160,
        SHD1,
        SHD260,
        SHD2,
        LHD60,
        LHD,
        SLHD60,
        SLHD,
        AUDIO,
    }

    public class Quality
    {
        private static Dictionary<QualityTypes, Quality> valuesList = null;
        public static Dictionary<QualityTypes, Quality> values()
        {
            if (valuesList == null)
            {
                valuesList = new Dictionary<QualityTypes, Quality>()
                {
                    { QualityTypes.Source   ,   new Quality("Source"    , "chunked"     , "source"      , 0.00)     },
                    { QualityTypes.QUADK60  ,   new Quality("4k60fps"   , "1080p60"     , "3840x2160"   , 60.000)   },
                    { QualityTypes.QUADK    ,   new Quality("4k30fps"   , "1080p30"     , "3840x2160"   , 30.000)   },
                    { QualityTypes.QHD4k60  ,   new Quality("2580p60fps", "1080p60fps"  , "2580x1080"   , 60.000)   },
                    { QualityTypes.QHD4k    ,   new Quality("2580p30fps", "1080p30"     , "2580x1080"   , 30.000)   },
                    { QualityTypes.QHD60    ,   new Quality("1440p60fps", "1080p60"     , "2560x1440"   , 60.000)   },
                    { QualityTypes.QHD      ,   new Quality("1440p30fps", "1080p30"     , "2560x1440"   , 60.000)   },
                    { QualityTypes.FHD60    ,   new Quality("1080p60fps", "1080p60"     , "1920x1080"   , 60.000)   },
                    { QualityTypes.FHD      ,   new Quality("1080p30fps", "1080p30"     , "1920x1080"   , 30.000)   },
                    { QualityTypes.FMHD60   ,   new Quality("936p60fps" , "936p60"      , "1664x936"    , 60.000)   },
                    { QualityTypes.FMHD     ,   new Quality("936p30fps" , "936p30"      , "1664x936"    , 30.000)   },
                    { QualityTypes.MHD60    ,   new Quality("900p60fps" , "900p60"      , "1600x900"    , 60.000)   },
                    { QualityTypes.MHD      ,   new Quality("900p30fps" , "900p30"      , "1600x900"    , 30.000)   },
                    { QualityTypes.HD60     ,   new Quality("720p60fps" , "720p60"      , "1280x720"    , 60.000)   },
                    { QualityTypes.HD       ,   new Quality("720p30fps" , "720p30"      , "1280x720"    , 30.000)   },
                    { QualityTypes.SHD160   ,   new Quality("480p60fps" , "480p60"      , "852x480"     , 60.000)   },
                    { QualityTypes.SHD1     ,   new Quality("480p30fps" , "480p30"      , "852x480"     , 30.000)   },
                    { QualityTypes.SHD260   ,   new Quality("360p60fps" , "360p60"      , "640x360"     , 60.000)   },
                    { QualityTypes.SHD2     ,   new Quality("360p30fps" , "360p30"      , "640x360"     , 30.000)   },
                    { QualityTypes.LHD60    ,   new Quality("160p60fps" , "160p60"      , "284x160"     , 60.000)   },
                    { QualityTypes.LHD      ,   new Quality("160p30fps" , "160p30"      , "284x160"     , 30.000)   },
                    { QualityTypes.SLHD60   ,   new Quality("144p60fps" , "144p60"      , "256×144"     , 60.000)   },
                    { QualityTypes.SLHD     ,   new Quality("144p30fps" , "144p30"      , "256×144"     , 30.000)   },
                    { QualityTypes.AUDIO    ,   new Quality("Audio only", "audio_only"  , "0x0"         , 0.000)    },

                };
            }

            return valuesList;
        }

        public string text;
        public string video;
        string resolution;
        double fps;
        Quality(string t, string v, string r, double f)
        {
            text = t;
            video = v;
            resolution = r;
            fps = f;
        }

        /**
         * This method searches through
         * the quality enum for the quality
         * enum which matches a given
         * video value.
         * @param qual      String value representing the video quality to search for.
         * @return Quality  Quality enum which matches the inputted string video value, or null if none were found.
         */
        public static Quality getQualityV(string qual)
        {
            foreach (Quality quality in Quality.values().Values)
            {
                if (quality.video.Equals(qual))
                {
                    return quality;
                }
            }
            return null;
        }

        /**
         * This method searches through the
         * quality enum for the quality enum
         * which matches a given resolution.
         * @param resolution    String value representing the resolution value to search for.
         * @return Quality      Quality enum which matches the inputted resolution value, or null if none is found.
         */
        public static Quality getQualityR(string resolution)
        {
            foreach (Quality quality in Quality.values().Values)
            {
                if (quality.resolution.Equals(resolution))
                {
                    return quality;
                }
            }
            return null;
        }

        /**
         * This method searches through the
         * quality enum for the quality enum
         * which matches a given resolution
         * and FPS value.
         * @param resolution    String value representing a resolution value to search for.
         * @param fps           Double value representing an FPS value to search for.
         * @return Quality      Quality enum which matches the given resolution and FPS values, or null if none was found.
         */
        public static Quality getQualityRF(string resolution, double fps)
        {
            foreach (Quality quality in Quality.values().Values)
            {
                if (quality.resolution.Equals(resolution) && quality.fps == fps)
                {
                    return quality;
                }
            }
            return null;
        }

        public override string ToString()
        {
            return text;
        }
    }
}