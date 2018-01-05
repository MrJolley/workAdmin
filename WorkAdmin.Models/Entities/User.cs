using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAdmin.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 50)]
        public string LoginName { get; set; }

        [StringLength(maximumLength: 100)]
        public string FullName { get; set; }

        [StringLength(maximumLength: 100)]
        public string EnglishName { get; set; }

        [StringLength(maximumLength: 100)]
        public string ChineseName { get; set; }

        public string EmailAddress { get; set; }

        public bool IsManager { get; set; }

        public bool? IsHeyiMember { get; set; }

        #region 增加职级，部门，项目，职位
        public string rankLevel { get; set; }

        public string department { get; set; }

        public string project { get; set; }

        public string position { get; set; }
        #endregion

        public bool IsExcluded { get; set; }

        public bool IsWorkingAtHome { get; set; }

        public DateTime CreatedTime { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UpdatedTime { get; set; }

        public string UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        #region Add as short letter name for user
        public string LetterName
        {
            get
            {
                return string.IsNullOrEmpty(EmailAddress) ?
                string.Empty : EmailAddress.Substring(0, EmailAddress.IndexOf("@")).Replace(".", " ").ToLower();
            }
        }
        #endregion
    }
}
