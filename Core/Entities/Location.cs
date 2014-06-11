using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Oclumen.Core.Entities
{
    [DataContract]
    public class Location
    {
        [Key]
        [DataMember]
        public Int64 id { get; set; }

        [NotMapped]
        [DataMember]
        public string[] coordinates
        {
            get
            {
                var coordinatesString = new String[2] { Longitude, Latitute };
                return coordinatesString;
            }
            set
            {
                if (value.Length == 2)
                {
                    value[0] = Longitude;
                    value[1] = Latitute;
                }
            }
        }

        [MaxLength(15)]
        public string Latitute { get; set; }

        [MaxLength(15)]
        public string Longitude { get; set; }

        [DataMember]
        public string type { get; set; }

        [Required]
        public virtual Tweet TweedId { get; set; }
    }
}
