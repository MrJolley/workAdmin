using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAdmin.Models.Entities
{
    public class WorkReportRecord
    {
        [Key]
        public int recordId { get; set; }

        public string userName { get; set; }

        public string type { get; set; }

        public DateTime recordDate { get; set; }

        public bool signIn { get; set; }
    }
}
