using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Oclumen.Core.DataContexts;
using Oclumen.Core.Entities;
using Oclumen.Core.Helpers;
using Oclumen.Core.LanguageProcessing;

namespace Oclumen.Core.Text.Classifiers
{
    public class NaiveBayes : IClassifier
    {
        /// <summary>
        /// Gets the text sentiment, automatically decides whether to use the tweet or retweet corpus counts
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="text">The text.</param>
        /// <param name="ngramCardinality">The ngram cardinality.</param>
        /// <param name="smoothingFactor">The smoothing factor.</param>
        /// <param name="isStemmed">if set to <c>true</c> [is stemmed].</param>
        /// <param name="dictionary">The dictionary used by the stemmer.</param>
        /// <param name="ngramDbSet">The ngram db set.</param>
        /// <param name="oclumenContext">The oclumen context.</param>
        /// <returns></returns>
        public Sentiment GetTextSentiment<TEntity>(string text, int ngramCardinality, decimal smoothingFactor, bool isStemmed, IDictionary<String, String> dictionary,
                                                            DbSet<TEntity> ngramDbSet, IOclumenContext oclumenContext, Dictionary<string, List<KeyValuePair<Sentiment, decimal>>> ngramDictionary = null) where TEntity : NgramBase
        {
            String originalText;
            TwitterTextUtility.GetRetweets(text, out originalText);

            if (originalText == String.Empty)
                return GetTextSentiment(text, true, ngramCardinality, smoothingFactor, isStemmed, dictionary, ngramDbSet, oclumenContext, ngramDictionary);
            else
                return GetTextSentiment(text, false, ngramCardinality, smoothingFactor, isStemmed, dictionary, ngramDbSet, oclumenContext, ngramDictionary);
        }

        public Sentiment GetTextSentiment<TEntity>(string text, bool isRetweet, int ngramCardinality,
                                                   decimal smoothingFactor, bool isStemmed, IDictionary<String, String> dictionary, DbSet<TEntity> ngramDbSet,
                                                   IOclumenContext oclumenContext, Dictionary<string, List<KeyValuePair<Sentiment, decimal>>> ngramDictionary = null) where TEntity : NgramBase
        {
            text = new TextCleaner(text).StripPunctuation().RemoveExcessSpaces().ToLower().ToString();

            IList<string> ngrams = NgramGenerator.GenerateNgrams(text, ngramCardinality);

            if (isStemmed)
                ngrams = Processor.StemNgram(ngrams, dictionary);

            var ngramCounts = new List<IList<KeyValuePair<Sentiment, decimal>>>(ngrams.Count);
            IList<KeyValuePair<Sentiment, decimal>> classCounts = GetClassCount(isRetweet, ngramCardinality,
                                                                                smoothingFactor, ngramDbSet,
                                                                                oclumenContext);

            // get the raw counts for each of the ngrams
            foreach (string ngram in ngrams)
            {
                ngramCounts.Add(GetNgramCount(ngram, isRetweet, ngramCardinality, smoothingFactor, ngramDbSet,
                                              oclumenContext, ngramDictionary));

                //Debug.WriteLine(ngram + " " + ngramCounts.Last().First(x => x.Key == Sentiment.Positive).Value + ", " + ngramCounts.Last().First(x => x.Key == Sentiment.Neutral).Value + ", " + ngramCounts.Last().First(x => x.Key == Sentiment.Negative).Value);
            }

            int vocabularySize = GetVocabularySize(isRetweet, ngramCardinality, ngramDbSet, oclumenContext);

            // ok now let's get the probabilities, combining the individual ngram probabilities
            // witht he probability of a given sentiment class
            var sentimentProb = GetNgramSentimentProbabilities(vocabularySize, ngramCounts, classCounts);

            return sentimentProb.Last().Key;
        }

        /// <summary>
        /// Gets the ngram sentiment probabilities based on the raw counts seen in the corpus.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="vocabularySize">Size of the vocabulary.</param>
        /// <param name="ngramCounts">The ngram counts.</param>
        /// <param name="classCounts">The class probabilities.</param>
        /// <returns></returns>
        protected IList<KeyValuePair<Sentiment, BigDecimal>> GetNgramSentimentProbabilities(int vocabularySize,
            IList<IList<KeyValuePair<Sentiment, decimal>>> ngramCounts,
            IList<KeyValuePair<Sentiment, decimal>> classCounts)
        {
            BigDecimal positiveProb = 1, neutralProb = 1, negativeProb = 1;
            decimal positiveClassCounts, neutralClassCounts, negativeClassCounts;
            decimal positiveClassProb, neutralClassProb, negativeClassProb;

            // caculate the sum of all the ngrams seen across all sentiments
            decimal classTotalCounts = classCounts.Sum(x => x.Value);

            var ngramSentimentProb = new List<KeyValuePair<Sentiment, BigDecimal>>(3);

            // get the number of times a given class is seen for a given ngram
            positiveClassCounts = classCounts.First(x => x.Key == Sentiment.Positive).Value;
            neutralClassCounts = classCounts.First(x => x.Key == Sentiment.Neutral).Value;
            negativeClassCounts = classCounts.First(x => x.Key == Sentiment.Negative).Value;

            foreach (var currentNgam in ngramCounts)
            {
                // for each ngram, calculate the product of the number of times the ngram was seen over the total number of times
                // that the sentiment it was in was seen, for all sentiments over all ngrams
                positiveProb = positiveProb + (currentNgam.First(x => x.Key == Sentiment.Positive).Value /
                               (positiveClassCounts + vocabularySize));
                neutralProb = neutralProb + (currentNgam.First(x => x.Key == Sentiment.Neutral).Value / (neutralClassCounts + vocabularySize));
                negativeProb = negativeProb + (currentNgam.First(x => x.Key == Sentiment.Negative).Value /
                               (negativeClassCounts + vocabularySize));
            }

            // calculate the probability of a given class
            positiveClassProb = positiveClassCounts / classTotalCounts;
            neutralClassProb = neutralClassCounts / classTotalCounts;
            negativeClassProb = negativeClassCounts / classTotalCounts;

            // take the individual ngram probabilities and multiply them by the overall probability of seeing a given class
            ngramSentimentProb.Add(new KeyValuePair<Sentiment, BigDecimal>(Sentiment.Positive, positiveProb * positiveClassProb));
            ngramSentimentProb.Add(new KeyValuePair<Sentiment, BigDecimal>(Sentiment.Neutral, neutralProb * neutralClassProb));
            ngramSentimentProb.Add(new KeyValuePair<Sentiment, BigDecimal>(Sentiment.Negative, negativeProb * negativeClassProb));

            if (positiveProb * positiveClassProb > negativeProb * negativeClassProb)
                Debug.WriteLine("hello world");

            ngramSentimentProb.Sort((x, y) => x.Value.CompareTo(y.Value));

            return ngramSentimentProb;
        }

        /// <summary>
        ///     Returns a list of key value pairs for a given sentiment and the raw counts seen.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="isRetweet">
        ///     if set to <c>true</c> [is retweet].
        /// </param>
        /// <param name="ngramCardinality">The ngram cardinality.</param>
        /// <param name="smoothingFactor">The smoothing factor.</param>
        /// <param name="ngramDbSet">The ngram db set.</param>
        /// <param name="oclumenContext">The oclumen context.</param>
        /// <returns></returns>
        /// <exception cref="System.DivideByZeroException">No corpus sentiment for this class.</exception>
        protected IList<KeyValuePair<Sentiment, decimal>> GetClassCount<TEntity>(bool isRetweet,
                                                                                 int ngramCardinality,
                                                                                 decimal smoothingFactor,
                                                                                 DbSet<TEntity> ngramDbSet,
                                                                                 IOclumenContext oclumenContext)
            where TEntity : NgramBase
        {
            var classProbabilityList = new List<KeyValuePair<Sentiment, decimal>>(3);

            decimal positiveSum, neutralSum, negativeSum;

            positiveSum = oclumenContext.RawTweets.Count(x => x.CorpusSentiment == (int)Sentiment.Positive && x.CorpusSentimentTimestamp != DateTime.MinValue);
            neutralSum = oclumenContext.RawTweets.Count(x => x.CorpusSentiment == (int)Sentiment.Neutral && x.CorpusSentimentTimestamp != DateTime.MinValue);
            negativeSum = oclumenContext.RawTweets.Count(x => x.CorpusSentiment == (int)Sentiment.Negative && x.CorpusSentimentTimestamp != DateTime.MinValue);

            classProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, positiveSum));
            classProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, neutralSum));
            classProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, negativeSum));

            return classProbabilityList;
        }

        /// <summary>
        ///     Returns the number of times that an ngram was seen in a positive, neutral an negative context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="text">The text.</param>
        /// <param name="isRetweet">
        ///     if set to <c>true</c> [is retweet].
        /// </param>
        /// <param name="ngramCardinality">The ngram cardinality.</param>
        /// <param name="smoothingFactor">The smoothing factor.</param>
        /// <param name="ngramDbSet">The ngram db set.</param>
        /// <param name="oclumenContext">The oclumen context.</param>
        /// <returns></returns>
        protected IList<KeyValuePair<Sentiment, decimal>> GetNgramCount<TEntity>(string text, bool isRetweet,
                                                                                 int ngramCardinality,
                                                                                 decimal smoothingFactor,
                                                                                 DbSet<TEntity> ngramDbSet,
                                                                                 IOclumenContext oclumenContext, Dictionary<string, List<KeyValuePair<Sentiment, decimal>>> ngramDictionary = null)
            where TEntity : NgramBase
        {
            //check to see if using ngramDictionary
            if (ngramDictionary != null)
            {
                text = text.ToLower();

                if (isRetweet)
                    text = text + "_rt";

                if (ngramDictionary.ContainsKey(text))
                    return ngramDictionary[text];
                else
                {
                    var ngramProbabilityListFake = new List<KeyValuePair<Sentiment, decimal>>(3);
                    ngramProbabilityListFake.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, smoothingFactor));
                    ngramProbabilityListFake.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, smoothingFactor));
                    ngramProbabilityListFake.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, smoothingFactor));

                    return ngramProbabilityListFake;
                }
            }

            var ngramProbabilityList = new List<KeyValuePair<Sentiment, decimal>>(3);

            decimal positiveCount = 0, neuturalCount = 0, negativeCount = 0;

            TEntity ngramRecord = ngramDbSet.FirstOrDefault(x => x.Text == text && x.Cardinality == ngramCardinality);

            if (ngramRecord != null)
            {
                if (isRetweet)
                {
                    positiveCount = ngramRecord.RtPositiveCount;
                    neuturalCount = ngramRecord.RtNeutralCount;
                    negativeCount = ngramRecord.RtNegativeCount;
                }
                else
                {
                    positiveCount = ngramRecord.PositiveCount;
                    neuturalCount = ngramRecord.NeutralCount;
                    negativeCount = ngramRecord.NegativeCount;
                }
            }

            positiveCount += smoothingFactor;
            neuturalCount += smoothingFactor;
            negativeCount += smoothingFactor;

            ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive, positiveCount));
            ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral, neuturalCount));
            ngramProbabilityList.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative, negativeCount));

            return ngramProbabilityList;
        }

        /// <summary>
        /// Gets the total number of distinct words in the corpus
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="isRetweet">if set to <c>true</c> [is retweet].</param>
        /// <param name="ngramCardinality">The ngram cardinality.</param>
        /// <param name="ngramDbSet">The ngram db set.</param>
        /// <param name="oclumenContext">The oclumen context.</param>
        /// <returns></returns>
        protected int GetVocabularySize<TEntity>(bool isRetweet, int ngramCardinality, DbSet<TEntity> ngramDbSet, IOclumenContext oclumenContext)
            where TEntity : NgramBase
        {
            if (isRetweet)
            {
                return ngramDbSet.Count(x => x.Cardinality == ngramCardinality && (x.RtPositiveCount != 0 || x.RtNeutralCount != 0 || x.RtNegativeCount != 0));
            }
            else
            {
                return ngramDbSet.Count(x => x.Cardinality == ngramCardinality && (x.PositiveCount != 0 || x.NeutralCount != 0 || x.NegativeCount != 0));
            }
        }

        /// <summary>
        ///     Sums the raw counts for sentiment collections.
        /// </summary>
        /// <param name="sentimentCounts">The sentiment counts.</param>
        /// <returns></returns>
        protected IList<KeyValuePair<Sentiment, decimal>> SumRawCounts(
            IList<KeyValuePair<Sentiment, decimal>> sentimentCounts)
        {
            var combinedProbabilities = new List<KeyValuePair<Sentiment, decimal>>(3);

            combinedProbabilities.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Positive,
                                                                           sentimentCounts.Where(
                                                                               x => x.Key == Sentiment.Positive)
                                                                                          .Sum(x => x.Value)));
            combinedProbabilities.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Neutral,
                                                                           sentimentCounts.Where(
                                                                               x => x.Key == Sentiment.Neutral)
                                                                                          .Sum(x => x.Value)));
            combinedProbabilities.Add(new KeyValuePair<Sentiment, decimal>(Sentiment.Negative,
                                                                           sentimentCounts.Where(
                                                                               x => x.Key == Sentiment.Negative)
                                                                                          .Sum(x => x.Value)));

            return combinedProbabilities;
        }
    }
}