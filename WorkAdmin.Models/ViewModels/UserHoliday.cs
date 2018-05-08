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
        /// 上一区间剩余的带薪假期总时间
        /// </summary>
        private double _beforePaidLeaveRemainingHours;
        public double BeforePaidLeaveRemainingHours { get => _beforePaidLeaveRemainingHours; set => _beforePaidLeaveRemainingHours = value; }

        /// <summary>
        /// 本区间剩余的带薪假期总时间
        /// </summary>
        private double _currentPaidLeaveRemainingHours;
        public double CurrentPaidLeaveRemainingHours { get => _currentPaidLeaveRemainingHours; set => _currentPaidLeaveRemainingHours = value; }

        /// <summary>
        /// 当前已使用的年假总和，包括上一区间剩余的年假
        /// </summary>
        private double _currentUsedPaidLeaveHours;
        public double CurrentUsedPaidLeaveHours { get => _currentUsedPaidLeaveHours; set => _currentUsedPaidLeaveHours = value; }

        /// <summary>
        /// 当前剩余的带薪假期总时间
        /// </summary>
        public double PaidLeaveRemainingHours { get => _beforePaidLeaveRemainingHours + _currentPaidLeaveRemainingHours - _currentUsedPaidLeaveHours; }

        /// <summary>
        /// 当前可用年假，包括上一区间剩余的年假和本区间可使用的年假
        /// </summary>
        public double CurrentAvailableRemainingHours
        {
            get
            {
                double availableMonth = (curDate.Year - _paidLeaveBeginDate.Year) * 12 + (curDate.Month - _paidLeaveBeginDate.Month) + 1 + 3; // 3 means a quarter holiday
                double num = curDate < _paidLeaveBeginDate ? 0 :
                    (curDate > _paidLeaveEndDate ? 12 :
                    (availableMonth > 12 ? 12 : availableMonth));
                var legalRemaining = Math.Round((num > 6 ? _beforePaidLeaveRemainingHours : _beforePaidLeaveRemainingHours / 2) + _currentPaidLeaveRemainingHours / 12 * num, 2);
                return legalRemaining > _currentUsedPaidLeaveHours ? (legalRemaining - _currentUsedPaidLeaveHours) : 0;
            }
        }
        #endregion
    }
}
