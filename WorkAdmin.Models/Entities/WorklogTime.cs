using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace WorkAdmin.Models.Entities
{
    public class WorklogTime
    {
        [Key]
        public int recordId { get; set; }

        public string userName { get; set; }

        public string rank { get; set; }

        public string department { get; set; }

        public string position { get; set; }

        public string project { get; set; }

        public double worklog { get; set; }

        [DataType(DataType.Date)]
        public DateTime logDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime updateDate { get; set; }

        public string updateUser { get; set; }

        public string portfolio
        {
            get { return string.Format("{0}*{1}*{2}", department, project, position); }
        }
    }
}
