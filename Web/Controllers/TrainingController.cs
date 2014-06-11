using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Oclumen.Core.Entities;
using Oclumen.Core.Text;
using Oclumen.Core.Text.Classifiers;
using Oclumen.Web.Models;

namespace Oclumen.Web.Controllers
{
    public class TrainingController : BaseController
    {
        //
        // GET: /Training/

        public ActionResult Index()
        {
            var tweet = GetNextTweet();

            return View(UpdateClassifier(new TrainerModel { CurrentTweet = tweet }));
        }

        private Tweet GetNextTweet()
        {
            Tweet tweet =
                OclumenContext.RawTweets.OrderBy(x => x.CorpusSentimentTimestamp)
                              .FirstOrDefault(x => x.CorpusSentimentTimestamp == DateTime.MinValue);
            return tweet;
        }

        public ActionResult RateTweet(long id, int sentiment)
        {
            var timestamp = DateTime.UtcNow;

            Tweet tweet = OclumenContext.RawTweets.First(x => x.id == id);
            String originalTweet;
            IList<string> retweets = TwitterTextUtility.GetRetweets(tweet.text, out originalTweet);

            // if the original tweet was not just a retweet
            if (!originalTweet.Equals(String.Empty))
                Processor.ProcessTweetTextForCorpus(originalTweet, false, Processor.GetSentimentEnum(sentiment),
                                                    timestamp, Dictionary, OclumenContext);

            OclumenContext.SaveChanges();

            //now process retweets
            foreach (String retweetedString in retweets)
                Processor.ProcessTweetTextForCorpus(retweetedString, true, Processor.GetSentimentEnum(sentiment),
                                                    timestamp, Dictionary, OclumenContext);

            // keep track of the sentiment used and when the manual training was actually done
            tweet.CorpusSentiment = sentiment;
            tweet.CorpusSentimentTimestamp = timestamp;

            // if the original tweet was just a retweet, lets update all the other possible retweets in the database
            if (originalTweet.Equals(String.Empty))
            {
                foreach (String retweetedString in retweets)
                {
                    String tweetText = retweetedString;
                    IQueryable<Tweet> sameTweet =
                        OclumenContext.RawTweets.Where(
                            x => x.text == tweetText && x.CorpusSentimentTimestamp == DateTime.MinValue);

                    foreach (Tweet tempTweet in sameTweet)
                    {
                        Processor.ProcessTweetTextForCorpus(tempTweet.text, true, Processor.GetSentimentEnum(sentiment),
                                                            timestamp, Dictionary, OclumenContext);
                        tempTweet.CorpusSentiment = sentiment;
                        tempTweet.CorpusSentimentTimestamp = timestamp;
                    }
                }
            }

            OclumenContext.SaveChanges();

            var nextTweet = GetNextTweet();

            return View("Index", UpdateClassifier(new TrainerModel { CurrentTweet = nextTweet }));
        }

        public ActionResult Delete(long id)
        {
            var tweetText = OclumenContext.RawTweets.First(x => x.id == id).text;

            var foundTweets = OclumenContext.RawTweets.Where(x => x.text == tweetText);

            foreach (var tweet in foundTweets)
                OclumenContext.RawTweets.Remove(tweet);

            OclumenContext.SaveChanges();

            var nextTweet = GetNextTweet();

            return View("Index", UpdateClassifier(new TrainerModel { CurrentTweet = nextTweet }));
        }

        /// <summary>
        /// Updates the classifier guess on the training page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        private TrainerModel UpdateClassifier(TrainerModel model)
        {
            // ask the naive bayes classifier what it thinks
            var classifier = new NaiveBayes();
            model.UnigramSentiment = classifier.GetTextSentiment(model.CurrentTweet.text, 1, 1, false, null, OclumenContext.BasicNgrams, OclumenContext);
            model.BigramSentiment = classifier.GetTextSentiment(model.CurrentTweet.text, 2, 1, false, null, OclumenContext.BasicNgrams, OclumenContext);

            model.UnigramSentimentStemmed = classifier.GetTextSentiment(model.CurrentTweet.text, 1, 1, true, Dictionary, OclumenContext.StemmedNgrams, OclumenContext);
            model.BigramSentimentStemmed = classifier.GetTextSentiment(model.CurrentTweet.text, 2, 1, true, Dictionary, OclumenContext.StemmedNgrams, OclumenContext);

            return model;
        }
    }
}