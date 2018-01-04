using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using WorkAdmin.Models.Entities;

namespace WorkAdmin.Models.ViewModels
{
    public class WorkReportViewModel
    {
        public WorkReportViewModel()
        {
            Month = DateTime.Now.ToString("MM/yyyy");
        }

        public string Month { get; set; }

        public string SortField { get; set; }

        public ESortDirection SortDirection { get; set; }

        public string Filter { get; set; }

        public DataView WorkReportDataView { get; set; }

        public DataView WorkReportAtHomeDataView { get; set; }

        public WorkReportProperty UploadProperty { get; set; }

        public List<string> UserAutoCompletionSource { get; set; }
    }
}
