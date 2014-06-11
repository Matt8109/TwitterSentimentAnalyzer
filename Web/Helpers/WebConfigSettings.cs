using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Oclumen.Crawler.Helpers;

namespace Oclumen.Web.Helpers
{
    public static class WebConfigSettings
    {
        public static String DefaultDictionary
        {
            get { return ConfigurationManager.AppSettings["DefaultDictionary"]; }
        }

        public static String TwitterUsername
        {
            get { return ConfigurationManager.AppSettings["TwitterUsername"]; }
        }

        public static String TwitterPassword
        {
            get { return ConfigurationManager.AppSettings["TwitterPassword"]; }
        }

        public static String TwitterStreamApiUrl
        {
            get { return ConfigurationManager.AppSettings["TwitterStreamApiUrl"]; }
        }

        public static String TwitterTrackKeywords
        {
            get { return ConfigurationManager.AppSettings["TwitterTrackKeywords"]; }
        }

        public static String ContextName
        {
            get { return ConfigurationManager.AppSettings["ContextName"]; }
        }

        public static int ProcessTweetThreadSleepTime
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["ProcessTweetThreadSleepTime"]);
            }
        }
    }
}