using System;
using System.Collections.Generic;
using System.Text;

namespace Oclumen.Core.LanguageProcessing
{
    public static class NgramGenerator
    {
        /// <summary>
        ///     Generates the ngrams from the given text that are of ngramSize.
        ///     That is if ngramSize == 1, we generate unigrams, if ngramSize==2
        ///     then we generate bigrams, and so on....
        ///     If the number of words is < ngramSize an empty list is returned.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ngramSize">Size of the ngram.</param>
        /// <returns></returns>
        public static IList<String> GenerateNgrams(string text, int ngramSize)
        {
            text = text.Trim();

            if (text == String.Empty)
                return new List<String>();

            string[] unigrams = text.Split(' ');

            // if the number of words is less then the min ngram size
            if (unigrams.Length < ngramSize)
                return new List<String>();

            var ngrams = new SortedDictionary<String, String>();
            IList<String> returnValues;

            for (int i = 0; i <= unigrams.Length - ngramSize; i++)
            {
                var newNgram = new StringBuilder();

                for (int j = i; j < i + ngramSize; j++)
                {
                    newNgram.Append(unigrams[j]);
                    newNgram.Append(" "); // add a space between words
                }

                string currentNgram = newNgram.ToString().Trim();

                if (!ngrams.ContainsKey(currentNgram))
                    ngrams.Add(currentNgram, "");
            }

            returnValues = new List<String>(ngrams.Count);

            foreach (String current in ngrams.Keys)
                returnValues.Add(current);

            return returnValues;
        }
    }
}