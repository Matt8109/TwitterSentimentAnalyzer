using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oclumen.Core.Entities;

namespace Oclumen.Web.Models
{
    public class OverviewModel
    {
        public IList<Tweet> RecentlyTaggedTweets { get; set; }

        public OverviewModel()
        {
            TopHashtags = new List<UseRecord>();
            TopRetweets = new List<UseRecord>();
            RecentSentiment = new List<UseRecord>();
        }

        public IList<UseRecord> TopHashtags { get; set; }
        public IList<UseRecord> TopRetweets { get; set; }
        public IList<UseRecord> RecentSentiment { get; set; } 
    }
}