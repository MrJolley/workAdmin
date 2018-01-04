using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace WorkAdmin.Models.ViewModels
{
    public class UploadWorkFileViewModel
    {
        public UploadWorkFileViewModel()
        {
            Month = DateTime.Now.ToString("MM/yyyy");
        }
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Work File Type")]
        public EWorkFileType WorkFileType { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [RegularExpression(@"^\d{1,2}/\d{4}$",ErrorMessage="Pls conform to format: mm/yyyy")]
        [Display(Name = "Month")]
        public string Month { get; set; }

        [Required(ErrorMessage = "{0} is required")]        
        [Display(Name="Upload File")]
        public HttpPostedFileBase UploadedFile { get; set; }       
    }
}
