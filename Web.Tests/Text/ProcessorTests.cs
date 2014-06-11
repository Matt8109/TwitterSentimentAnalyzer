using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oclumen.Core.Entities;
using Oclumen.Core.Text;

namespace Oclumen.Web.Tests.Text
{
    [TestClass]
    public class ProcessorTests
    {
        [TestMethod]
        public void TestGetNgramCardinality()
        {
            Assert.AreEqual(Processor.GetNgramCardinality(String.Empty), 0);
            Assert.AreEqual(Processor.GetNgramCardinality(" hello "), 1);
            Assert.AreEqual(Processor.GetNgramCardinality(" hello world  "), 2);
            Assert.AreEqual(Processor.GetNgramCardinality(" myst riven exile"), 3);
        }

        [TestMethod]
        public void TestUpdateNgramUsageCount()
        {
            var ngram = new BasicNgram();

            Processor.UpdateNgramUsageCount(ngram, false, Sentiment.Positive);
            Assert.AreEqual(ngram.PositiveCount, 1);
            Assert.AreEqual(ngram.NeutralCount, 0);
            Assert.AreEqual(ngram.NegativeCount, 0);
            Assert.AreEqual(ngram.RtPositiveCount, 0);
            Assert.AreEqual(ngram.RtPositiveCount, 0);
            Assert.AreEqual(ngram.RtPositiveCount, 0);

            Processor.UpdateNgramUsageCount(ngram, false, Sentiment.Positive);
            Processor.UpdateNgramUsageCount(ngram, false, Sentiment.Neutral);
            Processor.UpdateNgramUsageCount(ngram, false, Sentiment.Negative);
            Processor.UpdateNgramUsageCount(ngram, true, Sentiment.Positive);
            Processor.UpdateNgramUsageCount(ngram, true, Sentiment.Neutral);
            Processor.UpdateNgramUsageCount(ngram, true, Sentiment.Negative);

            Assert.AreEqual(ngram.PositiveCount, 2);
            Assert.AreEqual(ngram.NeutralCount, 1);
            Assert.AreEqual(ngram.NegativeCount, 1);
            Assert.AreEqual(ngram.RtPositiveCount, 1);
            Assert.AreEqual(ngram.RtPositiveCount, 1);
            Assert.AreEqual(ngram.RtPositiveCount, 1);
        }
    }
}