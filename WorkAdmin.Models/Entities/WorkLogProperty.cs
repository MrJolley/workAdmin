using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAdmin.Models.Entities
{
    public class WorkLogProperty
    {
        [Key]
        [Column(Order = 0)]
        public int Year { get; set; }

        [Key]
        [Column(Order = 1)]
        public int Month { get; set; }

        public string Comment { get; set; }

        [DataType(DataType.Date)]
        public DateTime AsOfDate { get; set; }

        public DateTime CreatedTime { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UpdatedTime { get; set; }

        public string UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
