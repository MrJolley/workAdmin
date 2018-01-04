using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace WorkAdmin.Models.ViewModels
{
    public class JiraWorkLogViewModel
    {
        public JiraWorkLogViewModel()
        {
            this.SearchInput = new JiraSearchInputViewModel();
            this.WorkLogDetails = new List<JiraWorkLogDetail>();
            this.WorkLogTimeUsedData = new List<JiraWorklogTimeUsed>();
            this.WorkLogMembers = new List<JiraWorklogMember>();
            this.JiraWorklogUsersName = new List<JiraWorklogUsersNameViewModel>();
            this.JiraWorklogProject = new List<JiraWorklogProjectViewModel>();
        }
        
        public JiraSearchInputViewModel SearchInput { get; set; }

        public List<JiraWorkLogDetail> WorkLogDetails { get; set; }

        public List<JiraWorklogTimeUsed> WorkLogTimeUsedData { get; set; }

        public List<JiraWorklogMember> WorkLogMembers { get; set; }

        public List<JiraWorklogUsersNameViewModel> JiraWorklogUsersName { get; set; }

        public List<JiraWorklogProjectViewModel> JiraWorklogProject { get; set; }
        
    }

    public class JiraSearchInputViewModel
    {
        public JiraSearchInputViewModel()
        {
            StartDate = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day;
            EndDate = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day;
            this.JiraWorklogUsersName = new List<JiraWorklogUsersNameViewModel>();
            this.JiraWorklogProject = new List<JiraWorklogProjectViewModel>();
            SelectedProject = "All";
        }
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Start Date")]
        public string StartDate { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "End Date")]
        public string EndDate { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Search Type")]
        public ESearchType SearchType { get; set; }

        [Display(Name = "Select User")]
        public string SelectedUser { get; set; }

        [Display(Name = "Select Project")]
        public string SelectedProject { get; set; }

        public List<JiraWorklogUsersNameViewModel> JiraWorklogUsersName { get; set; }

        public List<JiraWorklogProjectViewModel> JiraWorklogProject { get; set; }
    }

   
}
