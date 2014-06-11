using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oclumen.Core.Entities
{
    abstract public class NgramBase
    {
        [Key]
        [MaxLength(140)]
        public String Text { get; set; }

        [Required]
        public int Cardinality { get; set; }

        [Required]
        public int PositiveCount { get; set; }

        [Required]
        public int NeutralCount { get; set; }

        [Required]
        public int NegativeCount { get; set; }

        [Required]
        public int RtPositiveCount { get; set; }

        [Required]
        public int RtNeutralCount { get; set; }

        [Required]
        public int RtNegativeCount { get; set; }
    }
}
