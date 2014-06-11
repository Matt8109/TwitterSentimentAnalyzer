using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oclumen.Core.Entities
{
    public class BaseUseRecord
    {
        [Key]
        public Int64 Key { get; set; }

        [MaxLength(1024)]
        public String Tag { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime Timestamp { get; set; }
    }
}
