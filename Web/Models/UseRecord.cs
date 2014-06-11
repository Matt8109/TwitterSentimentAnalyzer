using System;

namespace Oclumen.Web.Models
{
    public class UseRecord
    {
        public String Tag { get; set; }
        private int Positive { get; set; }
        private int Neutral { get; set; }
        private int Negative { get; set; }
        private float Sum { get; set; }

        public UseRecord(string tag, int positive, int neutral, int negative)
        {
            Tag = tag;
            Positive = positive;
            Neutral = neutral;
            Negative = negative;

            Sum = positive + neutral + negative;
        }

        public int PositivePercent { get { return (int)(Positive / Sum * 100); } }
        public int NeutralPercent { get { return 100 - PositivePercent - NegativePercent; } } // lean towards neutral
        public int NegativePercent { get { return (int)(Negative / Sum * 100); } }
    }
}