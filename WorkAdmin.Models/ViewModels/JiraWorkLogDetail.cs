using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAdmin.Models.ViewModels
{
    public class JiraWorkLogDetail
    {
        public JiraWorkLogDetail() 
        {
            SortDirection = ESortDirection.Desending;
        }
        public string DisplayName { get; set; }

        public string WorkLogBody { get; set; }

        public decimal? TimeWorked { get; set;}

        public string UpdateTime { get; set; }

        public string StartTime { get; set; }

        public string Project { get; set; }

        public double TimeWorkedInHours
        {
            get
            {
                if (TimeWorked.HasValue) return (double)TimeWorked.Value / 3600;
                else return default(double);
            }
        }

        public string SortBy { get; set; }

        public ESortDirection SortDirection { get; set; }       
    }
}
