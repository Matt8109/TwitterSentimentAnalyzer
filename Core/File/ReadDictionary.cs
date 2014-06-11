using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oclumen.Core.File
{
    public static class ReadDictionary
    {
        public static void BuildDictionary(IDictionary<string, string> dictionary, string filePath)
        {
            string currentLine;
            var reader = new StreamReader(filePath);

            while ((currentLine = reader.ReadLine()) != null)
            {
                var currentLineSplit = currentLine.Split(' ');

                if (currentLineSplit.Length < 2)
                    continue;

                dictionary.Add(currentLineSplit[0].ToLower(), String.Empty);
            }

            // close the stream
            reader.Close();
        }
    }
}
