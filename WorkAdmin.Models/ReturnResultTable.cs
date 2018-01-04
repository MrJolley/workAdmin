using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace WorkAdmin.Models
{
    public class ReturnResultTable
    {    
            public bool Succeeded { get; set; }
            public string ErrorMsg { get; set; }
            public string TableName { get; set; }
            public DataTable SalaryTable { get; set; }        
    }
}
