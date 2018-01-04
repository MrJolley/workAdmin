using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkAdmin.Models.Entities;

namespace WorkAdmin.Models.ViewModels
{
    public class UserListViewModel
    {
        public string Filter { get; set; }

        public List<User> Users { get; set; }

        public List<string> UserAutoCompletionSource { get; set; }
    }
}
