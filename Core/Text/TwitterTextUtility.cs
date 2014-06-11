using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Oclumen.Core.Text
{
    public static class TwitterTextUtility
    {
        /// <summary>
        /// Gets the re-tweeted tweets within a larger tweet.
        /// </summary>
        /// <param name="tweet">The tweet.</param>
        /// <param name="originalTweet">The original tweet.</param>
        /// <returns></returns>
        public static IList<String> GetRetweets(String tweet, out String originalTweet)
        {
            int count = 0;
            string[] retweets = Regex.Split(tweet, @"RT\s@\w*");
            var returnList = new List<String>();
            originalTweet = String.Empty;

            foreach (string currentSubTweet in retweets)
            {
                // ignore the first part of the tweet if it doesnt start with a retweet
                // so "truth RT @obama I am the president" doesnt capture "truth"
                if (count == 0 && tweet.Length > 4 && tweet.Substring(0, 4) != "RT @")
                {
                    originalTweet = currentSubTweet.Trim();
                    count++;
                    continue;
                }

                if (currentSubTweet != String.Empty)
                {
                    returnList.Add(currentSubTweet.Trim());
                }

                count++;
            }

            return returnList;
        }

        /// <summary>
        /// Gets the hashtags in a tweet.
        /// </summary>
        /// <param name="tweet">The tweet.</param>
        /// <returns></returns>
        public static IList<String> GetHashtags(String tweet)
        {
            return RegexPatternMatchToString(tweet, @"(?<![""'=])#[0-Z_]+\b");
        }

        /// <summary>
        /// Determines whether the specified tweet is a retweet.
        /// </summary>
        /// <param name="tweet">The tweet.</param>
        /// <returns>
        ///   <c>true</c> if the specified tweet is retweet; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRetweet(String tweet)
        {
            // TODO Fix me
            return false;
        }

        /// <summary>
        /// Returns all the matches in a given string for a given regex pattern.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="regexPattern">The regex pattern.</param>
        /// <returns></returns>
        private static IList<String> RegexPatternMatchToString(String text, String regexPattern)
        {
            MatchCollection matchCollection = Regex.Matches(text, regexPattern, RegexOptions.IgnoreCase);
            var results = new List<String>(matchCollection.Count);

            foreach (Match currentMatch in matchCollection)
            {
                results.Add(currentMatch.Value);
            }

            return results;
        }
    }
}