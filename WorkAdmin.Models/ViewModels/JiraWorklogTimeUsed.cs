using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAdmin.Models.ViewModels
{
    public class JiraWorklogTimeUsed
    {
        public string Project { get; set; }

        public string DisplayName { get; set; }

        public decimal? TimeWorked { get; set; }

        public double TimeWorkedInHours
        {
            get
            {
                if (TimeWorked.HasValue) return (double)TimeWorked.Value / 3600;
                else return default(double);
            }
        }
    }
}
