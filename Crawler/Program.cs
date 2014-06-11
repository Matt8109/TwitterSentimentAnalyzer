using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Oclumen.Core.Entities;
using Oclumen.Core.File;
using Oclumen.Core.Helpers;
using Oclumen.Core.Text;
using Oclumen.Core.Text.Classifiers;
using Oclumen.Core.Twitter;
using Oclumen.Crawler.DataContexts;
using Oclumen.Crawler.Helpers;

namespace Oclumen.Crawler
{
    class Program
    {
        private static Queue<string> UnprocessedHashtags { get; set; }
        private static Queue<string> UnprocessedRetweets { get; set; }
        private static Queue<string> UnprocessedTweets { get; set; }
        private static Thread StreamReaderThread { get; set; }
        private static Thread StreamProcessorThread { get; set; }
        private static Thread StatusMessageThread { get; set; }
        private static Thread TweetClassifierThread { get; set; }
        private static Thread HashtagRetweetProcessorThread { get; set; }
        private static Object _syncRoot = new Object();
        private static Object _syncRootOutput = new Object();
        private static CrawlerStatus CrawlerStatus { get; set; }
        private static IDictionary<String, String> Dictionary { get; set; }
        private static IDictionary<String, NgramBase> ProcessedNgrams { get; set; }

        static void Main(string[] args)
        {
            CrawlerStatus = new CrawlerStatus()
                {
                    KeepRunning = true
                };

            UnprocessedHashtags = new Queue<string>();
            UnprocessedRetweets = new Queue<string>();
            UnprocessedTweets = new Queue<string>();

            StreamReaderThread = new Thread(ReadTweets);
            StreamReaderThread.Start();

            Console.WriteLine("");
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("Reading dictionary...");
            LoadDictionary();
            Console.WriteLine(Dictionary.Count + " dictionary entries read.");
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("");

            StreamProcessorThread = new Thread(ProcessTweets);
            StreamProcessorThread.Start();

            StatusMessageThread = new Thread(PrintStatusMessage);
            StatusMessageThread.Start();

            TweetClassifierThread = new Thread(TweetClassifier);
            TweetClassifierThread.Start();

            HashtagRetweetProcessorThread = new Thread(HashtagRetweetProcessor);
            HashtagRetweetProcessorThread.Start();

            Console.ReadLine();
            CrawlerStatus.KeepRunning = false;

            lock (_syncRootOutput)
            {
                Console.WriteLine("");
                Console.WriteLine("Stopping Threads.....");
                Console.WriteLine("");
                Console.WriteLine("Attempting stop Tweet Classifier....");
            }

            TweetClassifierThread.Join();

            lock (_syncRootOutput)
            {
                Console.WriteLine("Attempting stop Stream Reader....");
            }
            StreamReaderThread.Join();

            lock (_syncRootOutput)
            {
                Console.WriteLine("Attempting stop Stream Processor....");
            }
            StreamProcessorThread.Join();

            lock (_syncRootOutput)
            {
                Console.WriteLine("Attempting stop Status Message....");
            }
            StatusMessageThread.Join();

            lock (_syncRootOutput)
            {
                Console.WriteLine("Attempting stop Hashtag Retweet Processor....");
            }
            HashtagRetweetProcessorThread.Join();
        }

        public static void ReadTweets()
        {
            var twitterStreamClient = new TwitterStreamClient(UnprocessedTweets, CrawlerStatus, ref _syncRoot);

            twitterStreamClient.ConsumeStream(AppConfigSettings.TwitterUsername, AppConfigSettings.TwitterPassword,
                                              AppConfigSettings.TwitterTrackKeywords,
                                              AppConfigSettings.TwitterStreamApiUrl);
        }

        public static void TweetClassifier()
        {
            var context = new OclumenContext();
            var classifier = new NaiveBayes();
            var smoothingFactor = 1;

            var unigrams = new Dictionary<string, List<KeyValuePair<Sentiment, decimal>>>();
            var unigramsSt = new Dictionary<string, List<KeyValuePair<Sentiment, decimal>>>();
            var bigrams = new Dictionary<string, List<KeyValuePair<Sentiment, decimal>>>();
            var bigramsSt = new Dictionary<string, List<KeyValuePair<Sentiment, decimal>>>();

            foreach (var ngram in context.BasicNgrams.Where(x => x.Cardinality == 1))
            {
                var ngramProbabilityList = new List<KeyValuePair<Sentiment, decimal>>(3);
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, ngram.PositiveCount+ smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, ngram.NeutralCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, ngram.NegativeCount + smoothingFactor));

                unigrams.Add(ngram.Text.ToLower(), ngramProbabilityList);

                ngramProbabilityList = new List<KeyValuePair<Sentiment, decimal>>(3);
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, ngram.RtPositiveCount+ smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, ngram.RtNeutralCount+ smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, ngram.RtNegativeCount+ smoothingFactor));

                unigrams.Add(ngram.Text.ToLower() + "_rt", ngramProbabilityList);
            }

            foreach (var ngram in context.StemmedNgrams.Where(x => x.Cardinality == 1))
            {
                var ngramProbabilityList = new List<KeyValuePair<Sentiment, decimal>>(3);
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, ngram.PositiveCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, ngram.NeutralCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, ngram.NegativeCount + smoothingFactor));

                unigramsSt.Add(ngram.Text.ToLower(), ngramProbabilityList);

                ngramProbabilityList = new List<KeyValuePair<Sentiment, decimal>>(3);
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, ngram.RtPositiveCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, ngram.RtNeutralCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, ngram.RtNegativeCount + smoothingFactor));

                unigramsSt.Add(ngram.Text.ToLower() + "_rt", ngramProbabilityList);
            }

            foreach (var ngram in context.BasicNgrams.Where(x => x.Cardinality == 2))
            {
                var ngramProbabilityList = new List<KeyValuePair<Sentiment, decimal>>(3);
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, ngram.PositiveCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, ngram.NeutralCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, ngram.NegativeCount + smoothingFactor));

                bigrams.Add(ngram.Text.ToLower(), ngramProbabilityList);

                ngramProbabilityList = new List<KeyValuePair<Sentiment, decimal>>(3);
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, ngram.RtPositiveCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, ngram.RtNeutralCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, ngram.RtNegativeCount + smoothingFactor));

                bigrams.Add(ngram.Text.ToLower() + "_rt", ngramProbabilityList);
            }

            foreach (var ngram in context.StemmedNgrams.Where(x => x.Cardinality == 2))
            {
                var ngramProbabilityList = new List<KeyValuePair<Sentiment, decimal>>(3);
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, ngram.PositiveCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, ngram.NeutralCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, ngram.NegativeCount + smoothingFactor));

                bigramsSt.Add(ngram.Text.ToLower(), ngramProbabilityList);

                ngramProbabilityList = new List<KeyValuePair<Sentiment, decimal>>(3);
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, ngram.RtPositiveCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, ngram.RtNeutralCount + smoothingFactor));
                ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, ngram.RtNegativeCount + smoothingFactor));

                bigramsSt.Add(ngram.Text.ToLower() + "_rt", ngramProbabilityList);
            }


            int sleepTime = AppConfigSettings.ProcessTweetThreadSleepTime;
            while (CrawlerStatus.KeepRunning)
            {
                var tweetToProcess = context.RawTweets.First(x => x.AutoSentimentTimestamp == DateTime.MinValue);

                if (tweetToProcess == null)
                {
                    Thread.Sleep(sleepTime);
                    continue;
                }

                // add the hashtags to process queue
                String originalTweet;
                var hashtags = TwitterTextUtility.GetHashtags(tweetToProcess.text);
                var retweets = TwitterTextUtility.GetRetweets(tweetToProcess.text, out originalTweet);

                lock (_syncRoot)
                {
                    foreach (var hashtag in hashtags)
                        UnprocessedHashtags.Enqueue(hashtag);

                    foreach (var retweet in retweets)
                        UnprocessedRetweets.Enqueue(retweet);
                }

                // auto classify the tweets
                tweetToProcess.AutoUnigram = (int)classifier.GetTextSentiment(tweetToProcess.text, 1, 1, false, Dictionary, context.BasicNgrams, context, unigrams);
                tweetToProcess.AutoUnigramStemmed = (int)classifier.GetTextSentiment(tweetToProcess.text, 1, 1, true, Dictionary, context.StemmedNgrams, context, unigramsSt);

                tweetToProcess.AutoBigram = (int)classifier.GetTextSentiment(tweetToProcess.text, 2, 1, false, Dictionary, context.BasicNgrams, context, bigrams);
                tweetToProcess.AutoBigramStemmed = (int)classifier.GetTextSentiment(tweetToProcess.text, 2, 1, true, Dictionary, context.StemmedNgrams, context, bigramsSt);

                tweetToProcess.AutoSentimentTimestamp = DateTime.UtcNow;

                context.SaveChanges();
            }
        }

        public static void ProcessTweets()
        {
            int sleepTime = AppConfigSettings.ProcessTweetThreadSleepTime;

            while (CrawlerStatus.KeepRunning)
            {
                if (UnprocessedTweets.Count != 0)
                {
                    String currentTweet = string.Empty;
                    lock (_syncRoot)
                    {
                        currentTweet = UnprocessedTweets.Dequeue();
                    }

                    MessageProcess(currentTweet, true);
                }
                else
                {
                    Thread.Sleep(sleepTime);
                }
            }

            //clear out the queue before quitting
            while (UnprocessedTweets.Count != 0)
            {
                String currentTweet = string.Empty;
                lock (_syncRoot)
                {
                    currentTweet = UnprocessedTweets.Dequeue();
                }

                MessageProcess(currentTweet, false);
            }
        }

        public static void MessageProcess(String tweet, bool printTweet)
        {
            var status = new Tweet();
            var json = new DataContractJsonSerializer(status.GetType());

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(tweet);
                var stream = new MemoryStream(byteArray);
                status = json.ReadObject(stream) as Tweet;

                if (printTweet)
                {
                    lock (_syncRootOutput)
                    {
                        Console.WriteLine("");
                        Console.WriteLine(status.user.name + " tweeted: " + status.text);
                        Console.WriteLine("");
                    }
                }

                var context = new OclumenContext();
                var account = context.TwitterAccounts.FirstOrDefault(x => x.id == status.user.id);

                if (account == null)
                {
                    context.TwitterAccounts.Add(status.user);
                    context.TwitterAccounts.FirstOrDefault(x => x.id == status.user.id);
                    context.SaveChanges();
                    status.user = context.TwitterAccounts.FirstOrDefault(x => x.id == status.user.id);
                }

                status.AutoSentimentTimestamp = DateTime.MinValue;
                status.CorpusSentimentTimestamp = DateTime.MinValue;

                context.RawTweets.Add(status);
                if (status.geo != null)
                {
                    status.geo = context.Locations.Add(status.geo);
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void HashtagRetweetProcessor()
        {
            var context = new OclumenContext();

            var localUnprocessedHashtags = new Queue<string>();
            var localUnprocessedRetweets = new Queue<string>();

            int sleepTime = AppConfigSettings.ProcessTweetThreadSleepTime;
            while (CrawlerStatus.KeepRunning)
            {
                // grab from the producer queue and put into our own local consumer queue
                lock (_syncRoot)
                {
                    while (UnprocessedHashtags.Count != 0)
                        localUnprocessedHashtags.Enqueue(UnprocessedHashtags.Dequeue());

                    while (UnprocessedRetweets.Count != 0)
                        localUnprocessedRetweets.Enqueue(UnprocessedRetweets.Dequeue());
                }

                // if we didnt get anything from either queue, sleep for a bit
                if (localUnprocessedHashtags.Count == 0 && localUnprocessedRetweets.Count == 0)
                {
                    Thread.Sleep(sleepTime);
                    continue;
                }

                var now = DateTime.UtcNow;

                // ok, lets add the recently acquired data to our dataset
                while (localUnprocessedHashtags.Count != 0)
                    context.HashtagUseRecords.Add(new HashtagUseRecord()
                        {
                            Tag = localUnprocessedHashtags.Dequeue(),
                            Timestamp = now
                        });

                while (localUnprocessedRetweets.Count != 0)
                {
                    var currentRetweet = localUnprocessedRetweets.Dequeue();

                    //remove preceding :
                    if (currentRetweet.Length >= 1 && currentRetweet.Substring(0, 1) == ":")
                    {
                        currentRetweet = currentRetweet.Substring(1, currentRetweet.Length - 1).Trim();
                    }

                    // remove any proceding rt
                    if (currentRetweet.Length >= 3 && currentRetweet.Substring(0, 3) == "RT ")
                    {
                        currentRetweet = currentRetweet.Substring(3, currentRetweet.Length - 3).Trim();
                    }

                    // sometimes the rt function produces an empty string with just : so we might want to skip that
                    if (currentRetweet != String.Empty)
                    {
                        context.RetweetUseRecords.Add(new RetweetUseRecord()
                            {
                                Tag = currentRetweet,
                                Timestamp = now
                            });
                    }
                }

                context.SaveChanges();
            }
        }

        public static void PrintStatusMessage()
        {
            while (CrawlerStatus.KeepRunning || UnprocessedTweets.Count != 0)
            {
                lock (_syncRootOutput)
                {
                    Console.WriteLine("");
                    Console.WriteLine("---------------------------------------------------");
                    Console.WriteLine("Unprocessed Tweet Queue Length: " + UnprocessedTweets.Count);
                    Console.WriteLine("Unprocedded Hashtag Queue Length: " + UnprocessedHashtags.Count);
                    Console.WriteLine("Unprocedded Rewteet Queue Length: " + UnprocessedRetweets.Count);
                    Console.WriteLine("---------------------------------------------------");
                    Console.WriteLine("");
                }
                Thread.Sleep(500);
            }
        }

        public static void LoadDictionary()
        {
            Dictionary = new Dictionary<string, string>();
            ReadDictionary.BuildDictionary(Dictionary, AppConfigSettings.DefaultDictionary);
        }
    }
}
