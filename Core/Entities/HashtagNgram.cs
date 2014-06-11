using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oclumen.Core.Entities
{
    public class HashtagNgram : NgramBase
    {
        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime FirstSeen { get; set; }

    }
}
