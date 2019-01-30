using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAdmin.Models.ViewModels
{
    public class UserHoliday
    {
        #region constructor
        public UserHoliday()
        {
            //UserHolidayDetail = new List<HolidayDetail>();
        }
        #endregion

        #region field && property
        private DateTime curDate = DateTime.Now;

        private string _staffName;
        public string StaffName { get => _staffName; set => _staffName = value; }

        private string _staffEmail;
        public string StaffEmail { get => _staffEmail; set => _staffEmail = value; }

        /// <summary>
        /// 年假区间起始日期
        /// </summary>
        private DateTime _paidLeaveBeginDate;
        public DateTime PaidLeaveBeginDate { get => _paidLeaveBeginDate; set => _paidLeaveBeginDate = value; }

        /// <summary>
        /// 年假区间截止日期
        /// </summary>
        private DateTime _paidLeaveEndDate;
        public DateTime PaidLeaveEndDate { get => _paidLeaveEndDate; set => _paidLeaveEndDate = value; }

        /// <summary>
        /// 上一区间剩余假期总时间
        /// </summary>
        private double _beforeRemainingHours;
        public double BeforeRemainingHours { get => _beforeRemainingHours; set => _beforeRemainingHours = value; }

        /// <summary>
        /// 当前区间法定年假
        /// </summary>
        private double _currentLegalHours;
        public double CurrentLegalHours { get => _currentLegalHours; set => _currentLegalHours = value; }

        /// <summary>
        /// 当前区间福利年假
        /// </summary>
        private double _currentWelfareHours;
        public double CurrentWelfareHours { get => _currentWelfareHours; set => _currentWelfareHours = value; }

        /// <summary>
        /// 当前已使用的年假总和，包括上一区间剩余的年假，法定年假，福利年假
        /// </summary>
        private double _currentUsedHours;
        public double CurrentUsedHours { get => _currentUsedHours; set => _currentUsedHours = value; }

        /// <summary>
        /// 当前剩余的假期总时间
        /// </summary>
        public double TotalRemainingHours
        {
            get =>
                _beforeRemainingHours +
                _currentLegalHours +
                _currentWelfareHours -
                _currentUsedHours;
        }

        /// <summary>
        /// 当前可用年假，包括上一区间剩余的年假和本区间可使用的年假
        /// </summary>
        public double CurrentAvailableRemainingHours
        {
            // XXX年的总年假（法定+福利）/ 12 * 当前几月份 + 上一区间剩余 - 当前已使用
            get
            {
                double available = (_currentLegalHours + _currentWelfareHours) / 12 * curDate.Month +
                    _beforeRemainingHours - _currentUsedHours;
                return available > 0 ? double.Parse(available.ToString("f2")) : 0;
            }
        }
        #endregion
    }
}
