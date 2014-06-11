using System;

/*
 * Code from: https://bitbucket.org/alski/englishstemmer, Accessed April 17, 2013
 * Author: Alski, http://alski.net/
 * Slight modifications made
 */

namespace Oclumen.Core.LanguageProcessing
{
    internal class WordRegion
    {
        public int End;
        public int Start;
        private string _text;

        public WordRegion()
        {
        }

        public WordRegion(int start, int end)
        {
            Start = start;
            End = end;
        }

        public string Text
        {
            get { return _text; }
        }


        internal bool Contains(int index)
        {
            return (index >= Start && index <= End);
        }

        internal void GenerateRegion(string text)
        {
            if (text.Length > Start)
                _text = text.Substring(Start, Math.Min(End, text.Length) - Start);
            else
                _text = String.Empty;
        }
    }
}