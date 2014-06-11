using System;
using System.Text.RegularExpressions;

namespace Oclumen.Core.Text
{
    /// <summary>
    ///     Fluent interface for cleaning up text from tweets.
    /// </summary>
    public class TextCleaner
    {
        private string _cleanedString;

        public TextCleaner(string cleanedString)
        {
            _cleanedString = cleanedString;
        }

        public TextCleaner ToLower()
        {
            _cleanedString = _cleanedString.ToLower();
            return this;
        }

        public TextCleaner StripPunctuation()
        {
            // twitter converts < and > to &lt; and &gt; pushing us well over 140 characters, 
            // we dont care about them so drop them
            _cleanedString = Regex.Replace(_cleanedString, "&lt;", String.Empty, RegexOptions.IgnoreCase);
            _cleanedString = Regex.Replace(_cleanedString, "&gt;", String.Empty, RegexOptions.IgnoreCase);

            //strip punctuation and emoji
            var regEx = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant );
            _cleanedString = regEx.Replace(_cleanedString, String.Empty);

            return this;
        }

        public TextCleaner RemoveExcessSpaces()
        {
            _cleanedString = _cleanedString.Trim();
            _cleanedString = Regex.Replace(_cleanedString, @"\s\s+", " ");

            return this;
        }

        public override string ToString()
        {
            return _cleanedString;
        }
    }
}