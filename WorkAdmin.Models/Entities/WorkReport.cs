using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAdmin.Models.Entities
{
    public class WorkReport
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [DataType(DataType.Date)]
        public DateTime AsOfDate { get; set; }

        public string UserType { get; set; }

        public bool MorningReportAbsent { get; set; }

        public bool NoonReportAbsent { get; set; }

        public bool? AfternoonReportAbsent { get; set; }

        public bool EveningReportAbsent { get; set; }

        public string EmployeeType { get; set; }

        public DateTime CreatedTime { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UpdatedTime { get; set; }

        public string UpdatedBy { get; set; }

        public int SortOrder { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual User User { get; set; }
    }
}
