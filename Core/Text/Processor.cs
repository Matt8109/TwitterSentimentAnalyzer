using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Oclumen.Core.DataContexts;
using Oclumen.Core.Entities;
using Oclumen.Core.LanguageProcessing;

namespace Oclumen.Core.Text
{
    public enum Sentiment
    {
        Positive = 2,
        Neutral = 0,
        Negative = -2
    }

    /// <summary>
    ///     Takes a partialTweetText, breaks it into ngrams and pushed them into the database with sentiment.
    /// </summary>
    public static class Processor
    {
        /// <summary>
        ///     Takes a partialTweetText, cleans it, breaks it into ngrams and stemmed ngrams.
        /// </summary>
        /// <param name="partialTweetText">The partialTweetText.</param>
        /// <param name="isRetweet">
        ///     if set to <c>true</c> [is retweet].
        /// </param>
        /// <param name="sentiment">The sentiment.</param>
        /// <param name="timestamp">The current time in UTC.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="context">The context.</param>
        public static void ProcessTweetTextForCorpus(string partialTweetText,
                                                     bool isRetweet,
                                                     Sentiment sentiment,
                                                     DateTime timestamp,
                                                     IDictionary<String, String> dictionary,
                                                     IOclumenContext context)
        {
            // first let's grab the hashtags
            UpdateHashtagSentiment(partialTweetText, isRetweet, sentiment, timestamp, context);

            // clean up the partialTweetText a bit
            string cleanedTweet =
                new TextCleaner(partialTweetText).StripPunctuation().RemoveExcessSpaces().ToLower().ToString();


            IList<string> unigrams = NgramGenerator.GenerateNgrams(cleanedTweet, 1);
            IList<string> bigrams = NgramGenerator.GenerateNgrams(cleanedTweet, 2);

            IList<string> stemmedNgrams = StemNgram(unigrams, dictionary);
            IList<string> stemmedBigrams = StemNgram(bigrams, dictionary);

            UpdateNgramsSentiment(unigrams, isRetweet, sentiment, false, context.BasicNgrams, context);
            UpdateNgramsSentiment(bigrams, isRetweet, sentiment, false, context.BasicNgrams, context);
            UpdateNgramsSentiment(stemmedNgrams, isRetweet, sentiment, true, context.StemmedNgrams, context);
            UpdateNgramsSentiment(stemmedBigrams, isRetweet, sentiment, true, context.StemmedNgrams, context);
        }

        /// <summary>
        ///     Updates the hashtag sentiment much in the same way general sentiment is recorded.
        /// </summary>
        /// <param name="tweetText">The tweet text.</param>
        /// <param name="isRetweet">
        ///     if set to <c>true</c> [is retweet].
        /// </param>
        /// <param name="sentiment">The sentiment.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="context">The context.</param>
        public static void UpdateHashtagSentiment(string tweetText, bool isRetweet, Sentiment sentiment,
                                                  DateTime timestamp, IOclumenContext context)
        {
            // clean up a bit, we can't use the one for processing tweets due to wanting to keep punctuation
            // and we cant do that after because we would have to call remove excess spaces again anyways
            IList<string> hashtags =
                TwitterTextUtility.GetHashtags(new TextCleaner(tweetText).RemoveExcessSpaces().ToLower().ToString());

            foreach (string hashtag in hashtags)
            {
                HashtagNgram hashtagNgram = context.Hashtags.FirstOrDefault(x => x.Text == hashtag);

                // create the new ngram entity if it isn't in our database already
                if (hashtagNgram == null)
                {
                    hashtagNgram = new HashtagNgram
                                       {
                                           Cardinality = 1,
                                           FirstSeen = timestamp,
                                           Text = hashtag
                                       };

                    context.Hashtags.Add(hashtagNgram);
                    context.SaveChanges();
                }

                UpdateNgramUsageCount(hashtagNgram, isRetweet, sentiment);
            }
        }

        /// <summary>
        ///     Updates the ngrams sentiment count based on the sentiment of the partialTweetText and whether
        ///     or not this ngram is part of the retweet.
        /// </summary>
        /// <param name="ngrams">The ngrams.</param>
        /// <param name="isRetweet">
        ///     if set to <c>true</c> [is retweet].
        /// </param>
        /// <param name="sentiment">The sentiment.</param>
        /// <param name="ngramDbSet">The ngram db set.</param>
        /// <param name="isStemmed">
        ///     if set to <c>true</c> [is stemmed].
        /// </param>
        /// <param name="context">The context.</param>
        public static void UpdateNgramsSentiment<TEntity>(IList<String> ngrams, bool isRetweet, Sentiment sentiment,
                                                          bool isStemmed, DbSet<TEntity> ngramDbSet,
                                                          IOclumenContext context) where TEntity : NgramBase
        {
            foreach (String currentNgram in ngrams)
            {
                NgramBase dbNgram = ngramDbSet.FirstOrDefault(x => x.Text == currentNgram);

                if (dbNgram == null)
                {
                    // ok we dont have this ngram in our database yet
                    NgramBase newNgram;

                    if (isStemmed)
                    {
                        newNgram = new StemmedNgram
                                       {
                                           Text = currentNgram,
                                           Cardinality = GetNgramCardinality(currentNgram)
                                       };
                    }
                    else
                    {
                        newNgram = new BasicNgram {Text = currentNgram, Cardinality = GetNgramCardinality(currentNgram)};
                    }

                    ngramDbSet.Add((TEntity) newNgram);
                    dbNgram = newNgram;
                    context.SaveChanges();
                }

                // update the usage count
                UpdateNgramUsageCount(dbNgram, isRetweet, sentiment);
            }
        }

        /// <summary>
        ///     Gets the number of words in a given ngram.
        /// </summary>
        /// <param name="ngram">The ngram.</param>
        /// <returns></returns>
        public static int GetNgramCardinality(String ngram)
        {
            ngram = ngram.Trim();

            if (ngram.Length == 0)
                return 0;

            return ngram.Trim().Split(' ').Length;
        }

        /// <summary>
        ///     Updates the ngram usage count based on the sentiment and whether or not
        ///     the partialTweetText is a rewteet.
        /// </summary>
        /// <param name="ngram">The ngram.</param>
        /// <param name="isRetweet">
        ///     if set to <c>true</c> [is retweet].
        /// </param>
        /// <param name="sentiment">The sentiment.</param>
        public static void UpdateNgramUsageCount(NgramBase ngram, bool isRetweet, Sentiment sentiment)
        {
            if (isRetweet)
            {
                if (sentiment.Equals(Sentiment.Positive))
                    ngram.RtPositiveCount++;
                else if (sentiment.Equals(Sentiment.Negative))
                    ngram.RtNegativeCount++;
                else
                    ngram.RtNeutralCount++;
            }
            else
            {
                if (sentiment.Equals(Sentiment.Positive))
                    ngram.PositiveCount++;
                else if (sentiment.Equals(Sentiment.Negative))
                    ngram.NegativeCount++;
                else
                    ngram.NeutralCount++;
            }
        }

        /// <summary>
        ///     Stems a list of ngrams. Only returns the stemmed version
        ///     of words that are actually in the dictionary.
        /// </summary>
        /// <param name="ngrams">The ngrams.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public static IList<String> StemNgram(IList<String> ngrams, IDictionary<String, String> dictionary)
        {
            var stemmedNgrams = new List<String>(ngrams.Count);

            foreach (String tempNgram in ngrams)
                stemmedNgrams.Add(StemNgram(tempNgram, dictionary));

            return stemmedNgrams;
        }

        /// <summary>
        ///     Stems the a string containing ngrams. Only returns the stemmed version
        ///     of words that are actually in the dictionary.
        /// </summary>
        /// <param name="ngram">The ngram.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public static String StemNgram(String ngram, IDictionary<String, String> dictionary)
        {
            var newNgram = new StringBuilder();
            string[] ngramsArray = ngram.Split(' ');

            foreach (string currentWord in ngramsArray)
            {
                var stemmer = new EnglishWordStemmer(currentWord);
                string stemmedNgram = stemmer.Stem;

                // only use the stemmed version if it is an actual word in our dictionary
                if (dictionary.ContainsKey(stemmedNgram))
                    newNgram.Append(stemmedNgram);
                else
                    newNgram.Append(currentWord);

                // space out our words
                newNgram.Append(" ");
            }

            return newNgram.ToString().Trim();
        }

        /// <summary>
        ///     Converts int version of sentiment to enum.
        /// </summary>
        /// <param name="sentiment">The sentiment.</param>
        /// <returns></returns>
        public static Sentiment GetSentimentEnum(int sentiment)
        {
            return (Sentiment) sentiment;
        }
    }
}