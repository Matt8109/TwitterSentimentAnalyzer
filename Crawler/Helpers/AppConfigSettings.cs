using System;
using System.Configuration;

//<add key="ConsumerKey" value="2VSQbcfKrk64gRKp10g"/>
// <add key="ConsumerSecret" value="2VSQbcfKrk64gRKp10g"/>
// <add key="RequestTokenUrl" value="https://api.twitter.com/oauth/request_token"/>
// <add key="AuthorizeUrl" value="https://api.twitter.com/oauth/authorize"/>
// <add key="AccessTokenUrl" value="https://api.twitter.com/oauth/access_token"/>

namespace Oclumen.Crawler.Helpers
{
    public class AppConfigSettings
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

        public static int DefaultTweetDisplayCount
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["DefaultTweetDisplayCount"]);
            }
        }
    }
}