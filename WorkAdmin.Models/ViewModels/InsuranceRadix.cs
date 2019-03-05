using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WorkAdmin.Models.ViewModels
{
    public class InsuranceRadix
    {
        [Display(Name = "统计年份")]
        public int Year { get; set; }
        = DateTime.Now.Year;

        public List<UserInfo> InsuranceRadixDetails { get; set; }
        = new List<UserInfo>();

        public class UserInfo
        {
            [Display(Name = "姓名")]
            [Required(ErrorMessage = "员工姓名为必填项")]
            public string ChineseName { get; set; }

            [Display(Name = "平均工资")]
            [Required(ErrorMessage = "员工平均工资为必填项")]
            public double AunualIncome { get; set; }

            [Display(Name = "邮箱")]
            [Required(ErrorMessage = "员工邮箱为必须项")]
            public string Email { get; set; }
        }
    }
}
