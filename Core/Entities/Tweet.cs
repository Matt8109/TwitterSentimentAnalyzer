using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Runtime.Serialization;

namespace Oclumen.Core.Entities
{
    [DataContract]
    public class Tweet
    {
        [Key]
        [DataMember]
        public Int64 id { get; set; }

        [DataMember]
        public string contributors { get; set; }

        public DateTimeOffset created_at_dt { get; set; }

        [DataMember]
        public string favorited { get; set; }

        [DataMember]
        [ForeignKey("id")]
        public virtual Location geo { get; set; }

        [DataMember]
        public string in_reply_to_screen_name { get; set; }

        [DataMember]
        public string in_reply_to_status_id { get; set; }

        [DataMember]
        public string in_reply_to_user_id { get; set; }

        [DataMember]
        public string source { get; set; }

        [DataMember]
        [MaxLength(1024)]
        public string text { get; set; }

        [DataMember]
        public string truncated { get; set; }

        [DataMember]
        public virtual TwitterAccount user { get; set; }

        [DataMember]
        public string created_at
        {
            get { return created_at_dt.ToString("ddd MMM dd HH:mm:ss zzz yyyy"); }
            set
            {
                created_at_dt = DateTimeOffset.ParseExact(value, "ddd MMM dd HH:mm:ss zzz yyyy",
                                                          CultureInfo.InvariantCulture);
            }
        }

        public int CorpusSentiment { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CorpusSentimentTimestamp { get; set; }

        public int AutoUnigram { get; set; }
        public int AutoUnigramStemmed { get; set; }
        public int AutoBigram { get; set; }
        public int AutoBigramStemmed { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime AutoSentimentTimestamp { get; set; }

        public int TestSentiment { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime TestSentimentTimestamp { get; set; }
    }
}