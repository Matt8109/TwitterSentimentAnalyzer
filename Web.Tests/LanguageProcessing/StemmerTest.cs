using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oclumen.Core.LanguageProcessing;

namespace Oclumen.Web.Tests.LanguageProcessing
{
    [TestClass]
    public class StemmerTest
    {
        [TestMethod]
        public void BasicStemmerTest()
        {
            var stemmer = new EnglishWordStemmer("");
            Assert.AreEqual(stemmer.Stem, "");

            stemmer = new EnglishWordStemmer("exceedingly");
            Assert.AreEqual(stemmer.Stem, "exceed");

            stemmer = new EnglishWordStemmer("running");
            Assert.AreEqual(stemmer.Stem, "run");

            // TODO: add dictionary check to results, return orig if not valid
            //stemmer = new EnglishWordStemmer("happier");
            //Assert.AreEqual(stemmer.Stem, "happy");

            stemmer = new EnglishWordStemmer("slim");
            Assert.AreEqual(stemmer.Stem, "slim");
        }
    }
}
