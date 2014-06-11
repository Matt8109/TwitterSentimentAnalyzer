using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oclumen.Core.Text;

namespace Oclumen.Web.Tests.Text
{
    [TestClass]
    public class TwitterTextUtilityTests
    {
        [TestMethod]
        public void TestGetRetweets()
        {
            String originalTweet;
            Assert.IsTrue(TwitterTextUtility.GetRetweets("", out originalTweet).Count == 0);

            var results = TwitterTextUtility.GetRetweets("truth RT @obama I am the president", out originalTweet);

            Assert.IsTrue(results.Count == 1);
            Assert.AreEqual(results[0], "I am the president");
            Assert.AreEqual(originalTweet, "truth");

            results = TwitterTextUtility.GetRetweets("RT @obama hello world RT @matt i like dogs RT @cnn some news", out originalTweet);

            Assert.IsTrue(results.Count == 3);
            Assert.AreEqual(results[0], "hello world");
            Assert.AreEqual(results[1], "i like dogs");
            Assert.AreEqual(results[2], "some news");
            Assert.AreEqual(originalTweet,String.Empty);
        }

        [TestMethod]
        public void TestGetHashtags()
        {
            Assert.IsTrue(TwitterTextUtility.GetHashtags("hello world, no hashtags to see here").Count == 0);

            var results = TwitterTextUtility.GetHashtags("#helloworld");

            Assert.IsTrue(results.Count==1);
            Assert.AreEqual(results[0], "#helloworld");

            results = TwitterTextUtility.GetHashtags("#one #two#three");
            Assert.IsTrue(results.Count==3);
            Assert.AreEqual(results[0], "#one");
            Assert.AreEqual(results[1], "#two");
            Assert.AreEqual(results[2], "#three");

            results = TwitterTextUtility.GetHashtags("#test this is a #one hi #two#three");
            Assert.IsTrue(results.Count == 4);
            Assert.AreEqual(results[0], "#test");
            Assert.AreEqual(results[1], "#one");
            Assert.AreEqual(results[2], "#two");
            Assert.AreEqual(results[3], "#three");
        }

        [TestMethod]
        public void TestGetHashtagsFromRetweets()
        {
            // the hashtag #youjustpulledanobama was popular at the time so I used it, no political reson behind it
            String originalTweet;
            var results = TwitterTextUtility.GetRetweets("truth #youjustpulledanobama#test RT @obama I am the president #testtwo #helloworld #youjustpulledanobama", out originalTweet);

            var hashtagResults = TwitterTextUtility.GetHashtags(originalTweet);

            Assert.IsTrue(hashtagResults.Contains("#youjustpulledanobama"));
            Assert.IsTrue(hashtagResults.Contains("#test"));
            Assert.IsTrue(hashtagResults.Count==2);

             hashtagResults = TwitterTextUtility.GetHashtags(results[0]);
             Assert.IsTrue(hashtagResults.Contains("#testtwo"));
             Assert.IsTrue(hashtagResults.Contains("#helloworld"));
             Assert.IsTrue(hashtagResults.Contains("#youjustpulledanobama"));
             Assert.IsTrue(hashtagResults.Count == 3);

             Assert.IsTrue(results.Count == 1);
        }
    }
}
