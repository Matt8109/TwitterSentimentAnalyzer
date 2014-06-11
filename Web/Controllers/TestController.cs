using System;
using System.Linq;
using System.Web.Mvc;
using Oclumen.Core.Entities;
using Oclumen.Web.Models;

namespace Oclumen.Web.Controllers
{
    public class TestController : BaseController
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            Tweet tweet = GetNextTweet();

            return View(new TestModel {CurrentTweet = GetNextTweet()});
        }

        public ActionResult RateTweet(long id, int sentiment)
        {
            DateTime timestamp = DateTime.UtcNow;

            Tweet tweet = OclumenContext.RawTweets.FirstOrDefault(x => x.id == id);

            if (tweet != null)
            {
                tweet.TestSentiment = sentiment;
                tweet.TestSentimentTimestamp = timestamp;
            }

            OclumenContext.SaveChanges();


            return View("Index", new TestModel {CurrentTweet = GetNextTweet()});
        }

        public ActionResult Delete(long id)
        {
            string tweetText = OclumenContext.RawTweets.First(x => x.id == id).text;

            IQueryable<Tweet> foundTweets = OclumenContext.RawTweets.Where(x => x.text == tweetText);

            foreach (Tweet tweet in foundTweets)
                OclumenContext.RawTweets.Remove(tweet);

            OclumenContext.SaveChanges();

            Tweet nextTweet = GetNextTweet();

            return View("Index", new TestModel {CurrentTweet = GetNextTweet()});
        }

        private Tweet GetNextTweet()
        {
            Tweet tweet =
                OclumenContext.RawTweets.FirstOrDefault(
                    x =>
                    x.CorpusSentimentTimestamp == DateTime.MinValue && x.AutoSentimentTimestamp != DateTime.MinValue &&
                    x.TestSentimentTimestamp == DateTime.MinValue);
            return tweet;
        }
    }
}