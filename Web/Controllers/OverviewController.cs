using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Configuration;
using System.Web.Mvc;
using Oclumen.Core.Entities;
using Oclumen.Crawler.Helpers;
using Oclumen.Web.Models;

namespace Oclumen.Web.Controllers
{
    public class OverviewController : BaseController
    {
        private const string HashtagSentimentKey = "tophashtagsentiment";
        private const string RetweetSentimentKey = "topretweetsentiment";
        private const string RecentSentimentKey = "recentsentiment";

        private static readonly MemoryCache cache = MemoryCache.Default;

        public ActionResult Index()
        {
            List<Tweet> recentTweets =
                OclumenContext.RawTweets.OrderByDescending(x => x.AutoSentimentTimestamp).Take(10).ToList();
            var model = new OverviewModel { RecentlyTaggedTweets = recentTweets };

            LoadTopHastagsAndRetweets(model);

            return View(model);
        }

        public void LoadTopHastagsAndRetweets(OverviewModel model)
        {
            object result = cache.Get(HashtagSentimentKey);

            if (result != null)
            {
                model.TopHashtags = (IList<UseRecord>)result;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["OclumenContext"].ToString()))
                {
                    conn.Open();

                    var cmd = new SqlCommand("TopHashtagSentiment", conn);

                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataReader rdr = cmd.ExecuteReader();

                    // read top hashtags
                    while (rdr.Read())
                    {
                        model.TopHashtags.Add(new UseRecord(rdr["tag"].ToString(), (int)rdr["Positive"], (int)rdr["Neutral"], (int)rdr["Negative"]));
                    }

                    rdr.Close();
                    conn.Dispose();
                }

                cache.Add(HashtagSentimentKey, model.TopHashtags, DateTimeOffset.UtcNow.AddMinutes(15));
            }

            // now load top retweets
            result = cache.Get(RetweetSentimentKey);

            if (result != null)
            {
                model.TopRetweets = (IList<UseRecord>)result;
            }
            else
            {
                using (var conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["OclumenContext"].ToString()))
                {
                    conn.Open();

                    var cmd = new SqlCommand("TopRetweetSentiment", conn);

                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataReader rdr = cmd.ExecuteReader();

                    // read top retweets
                    while (rdr.Read())
                    {
                        model.TopRetweets.Add(new UseRecord(rdr["tag"].ToString(), int.Parse(rdr["Positive"].ToString()),
                                                            int.Parse(rdr["Neutral"].ToString()),
                                                            int.Parse(rdr["Negative"].ToString())));
                    }

                    rdr.Close();
                    conn.Dispose();
                }

                cache.Add(RetweetSentimentKey, model.TopRetweets, DateTimeOffset.UtcNow.AddMinutes(15));
            }

            // now load top recent sentiment
            result = cache.Get(RecentSentimentKey);

            if (result != null)
            {
                model.RecentSentiment = (IList<UseRecord>)result;
            }
            else
            {
                using (var conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["OclumenContext"].ToString()))
                {
                    conn.Open();

                    var cmd = new SqlCommand("RecentSentiment", conn);

                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataReader rdr = cmd.ExecuteReader();

                    // read top retweets
                    rdr.Read();

                    model.RecentSentiment.Add(new UseRecord("Past Hour", int.Parse(rdr["Positive"].ToString()),
                                                        int.Parse(rdr["Neutral"].ToString()),
                                                        int.Parse(rdr["Negative"].ToString())));

                    model.RecentSentiment.Add(new UseRecord("One Hour Ago", int.Parse(rdr["PositiveOneHour"].ToString()),
                                int.Parse(rdr["NeutralOneHour"].ToString()),
                                int.Parse(rdr["NegativeOneHour"].ToString())));

                    model.RecentSentiment.Add(new UseRecord("Two Hours Ago", int.Parse(rdr["PositiveTwoHour"].ToString()),
                                int.Parse(rdr["NeutralTwoHour"].ToString()),
                                int.Parse(rdr["NegativeTwoHour"].ToString())));


                    rdr.Close();
                    conn.Dispose();
                }

                cache.Add(RecentSentimentKey, model.RecentSentiment, DateTimeOffset.UtcNow.AddMinutes(15));
            }
        }
    }
}