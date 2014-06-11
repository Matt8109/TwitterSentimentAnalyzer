using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oclumen.Core.LanguageProcessing;

namespace Oclumen.Web.Tests.LanguageProcessing
{
    [TestClass]
    public class NgramGeneratorTests
    {
        [TestMethod]
        public void NgramGeneratorTestEmpty()
        {
            // empty string
            Assert.IsTrue(NgramGenerator.GenerateNgrams("", 1).Count == 0);

            // test when no ngram can be generated because number of words < ngram size
            Assert.IsTrue(NgramGenerator.GenerateNgrams("hello world", 3).Count ==0);
        }

        [TestMethod]
        public void NgramGeneratorTestUnigram()
        {
            var ngrams = NgramGenerator.GenerateNgrams("hello", 1);

            Assert.IsTrue(ngrams.Contains("hello"));
            Assert.IsTrue(ngrams.Count==1);

            ngrams = NgramGenerator.GenerateNgrams("hello world test", 1);
            Assert.IsTrue(ngrams.Contains("hello"));
            Assert.IsTrue(ngrams.Contains("world"));
            Assert.IsTrue(ngrams.Contains("test"));
            Assert.IsTrue(ngrams.Count == 3);
        }

        [TestMethod]
        public void NgramGeneratorTestBigrams()
        {
            var ngrams = NgramGenerator.GenerateNgrams("hello world", 2);
            Assert.IsTrue(ngrams.Contains("hello world"));
            Assert.IsTrue(ngrams.Count == 1);

             ngrams = NgramGenerator.GenerateNgrams("hello world test one", 2);
            Assert.IsTrue(ngrams.Contains("hello world"));
            Assert.IsTrue(ngrams.Contains("world test"));
            Assert.IsTrue(ngrams.Contains("test one"));
            Assert.IsTrue(ngrams.Count == 3);
        }

        [TestMethod]
        public void NgramGeneratorTextDuplicateIgnore()
        {
            var ngrams = NgramGenerator.GenerateNgrams("hello world hello world", 2);
            Assert.IsTrue(ngrams.Contains("hello world"));
            Assert.IsTrue(ngrams.Contains("world hello"));
            Assert.IsTrue(ngrams.Count == 2);
        }
    }
}
