using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAdmin.Models
{
    public class ReturnResult<T>
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; }
        public T Data { get; set; }
    }

    public class ReturnResult
    {
        public bool Succeeded { get; set; }
        public string ErrorMsg { get; set; }
    }
}
