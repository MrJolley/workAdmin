using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Data;

namespace WorkAdmin.Models.ViewModels
{
    public class SalaryDetailsViewModel
    {
        public SalaryDetailsViewModel()
        {
            this.Month = ((0< DateTime.Now.Month && DateTime.Now.Month < 10)? "0" + DateTime.Now.Month.ToString(): DateTime.Now.Month.ToString()) + "/" + DateTime.Now.Year.ToString();
            this.Address = "hui.pan@sail-fs.com";
            this.SalaryTable = new DataTable();
            this.emailStyle = "Dear colleague,\n\n以下是本月发放的工资明细。如有任何问题，请及时联络我。\n\n";
            //this.succeed = new List<bool>();
            //this.SelectedName = new List<string>();
            //this.correctName = new List<string>();
            //this.errorName = new List<string>();
        }
       
        [Required(ErrorMessage = "{0} is required")]
        [RegularExpression(@"^\d{1,2}/\d{4}$",ErrorMessage="Pls confirm to format: mm/yyyy")]
        [Display(Name = "Month")]        
        public string Month { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Upload File")]
        public HttpPostedFileBase UploadedFile { get; set; }   

        [Required(ErrorMessage = "{0} is required")]       
        [Display(Name = "Address")]
        public string Address { get; set; }
       
        [Display(Name = "Password")]
        public string Password { get; set; }

        public DataTable SalaryTable { get; set; }
      
        public string TableName { get; set; }

        [Display(Name = "SelectedAddress")]
        public List<string> SelectedAddress { get; set; }

        [Display(Name = "SelectedName")]
        public List<string> SelectedName { get; set; }

        public List<bool> succeed { get; set; }

        public List<string> correctName { get; set; }

        public List<string> errorName { get; set; }

        public string emailStyle { get; set; }

    }
}
