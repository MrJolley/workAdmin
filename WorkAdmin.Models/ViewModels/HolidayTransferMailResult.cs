using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAdmin.Models.ViewModels
{
    public class HolidayTransferMailResult
    {
        public HolidayTransferMailResult()
        {
            SuccessList = new ConcurrentBag<string>();
            FailureList = new ConcurrentBag<string>();
            FailureMsg = new ConcurrentBag<string>();
        }

        
        public ConcurrentBag<string> SuccessList { get; set; }

        public ConcurrentBag<string> FailureList { get; set; }

        public ConcurrentBag<string> FailureMsg { get; set; }

        public int SuccessCount
        {
            get { return SuccessList.Count; }
        }

        public int FailureCount
        {
            get { return FailureList.Count; }
        }
    }
}
