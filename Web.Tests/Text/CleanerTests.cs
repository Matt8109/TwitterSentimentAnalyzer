using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oclumen.Core.Text;

namespace Oclumen.Web.Tests.Text
{
    [TestClass]
    public class CleanerTest
    {
        [TestMethod]
        public void TestToLower()
        {
            const string firstString = "Hello World";

            var textCleaner = new TextCleaner(firstString);

            Assert.AreEqual(textCleaner.ToLower().ToString(), firstString.ToLower());

            Assert.AreEqual(new TextCleaner("").ToLower().ToString(), "");
            Assert.AreEqual(new TextCleaner("hello").ToLower().ToString(), "hello");
        }

        [TestMethod]
        public void StripPunctuation()
        {
            Assert.AreEqual(new TextCleaner("Hello World!").StripPunctuation().ToString(), "Hello World");
            Assert.AreEqual(new TextCleaner("M()nd@y").StripPunctuation().ToString(), "Mndy");
            Assert.AreEqual(new TextCleaner(" !HelloWorld").StripPunctuation().ToString(), " HelloWorld");
        }

        [TestMethod]
        public void RemoveExcessSpaces()
        {
            Assert.AreEqual(new TextCleaner("Hello  World!").RemoveExcessSpaces().ToString(), "Hello World!");
            Assert.AreEqual(new TextCleaner("  M(  )nd@y$  ").RemoveExcessSpaces().ToString(), "M( )nd@y$");      // dont forget we also trim
            Assert.AreEqual(new TextCleaner("              ").RemoveExcessSpaces().ToString(), "");               // trim again, so this should be empty
        }

        [TestMethod]
        public void AllTogetherNow()
        {
            Assert.AreEqual(new TextCleaner("  Hello  @  World!  ").ToLower().StripPunctuation().RemoveExcessSpaces().ToString(), "hello world");
        }
    }
}
