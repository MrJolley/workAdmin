using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAdmin.Models.Entities
{
    public class UserPosition
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }

        public string category { get; set; }

        public int categoryId { get; set; }

        public string description { get; set; }

        public DateTime updateTime { get; set; }

        public string updateUser { get; set; }
    }
}
