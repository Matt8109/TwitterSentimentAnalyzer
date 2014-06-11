using Oclumen.Core.Entities;
using Oclumen.Core.Text;

namespace Oclumen.Web.Models
{
    public class TrainerModel
    {
        public Tweet CurrentTweet { get; set; }

        public Sentiment UnigramSentiment { get; set; }

        public Sentiment UnigramSentimentStemmed { get; set; }

        public Sentiment BigramSentiment { get; set; }

        public Sentiment BigramSentimentStemmed { get; set; }
    }
}