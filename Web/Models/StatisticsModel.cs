using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oclumen.Web.Models
{
    public class StatisticsModel
    {
        public int TotalTweets { get; set; }
        public int TweetsAwaitingProcessing { get; set; }
        public int TotalNgrams { get; set; }
        public int Unigrams { get; set; }
        public int Bigrams { get; set; }
        public int TotalNgramsStemmed { get; set; }
        public int StemmedUnigrams { get; set; }
        public int StemmedBigrams { get; set; }
        public int TotalHashtagNgrams { get; set; }
    }
}