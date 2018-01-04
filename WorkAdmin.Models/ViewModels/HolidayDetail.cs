using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAdmin.Models.ViewModels
{
    public class HolidayDetail
    {
        #region constructor
        public HolidayDetail()
        {
        }
        #endregion

        #region field && property
        /// <summary>
        /// 年假区间起始日期
        /// </summary>
        private DateTime _paidLeaveBeginDate;

        public DateTime PaidLeaveBeginDate
        {
            get { return _paidLeaveBeginDate; }
            set { _paidLeaveBeginDate = value; }
        }

        /// <summary>
        /// 年假区间截止日期
        /// </summary>
        private DateTime _paidLeaveEndDate;

        public DateTime PaidLeaveEndDate
        {
            get { return _paidLeaveEndDate; }
            set { _paidLeaveEndDate = value; }
        }

        /// <summary>
        /// 上一区间剩余的带薪假期时间
        /// </summary>
        private double _beforePaidLeaveRemainingHours;

        public double BeforePaidLeaveRemainingHours
        {
            get { return _beforePaidLeaveRemainingHours; }
            set { _beforePaidLeaveRemainingHours = value; }
        }

        /// <summary>
        /// 本区间剩余的带薪假期时间
        /// </summary>
        private double _currentPaidLeaveRemainingHours;

        public double CurrentPaidLeaveRemainingHours
        {
            get { return _currentPaidLeaveRemainingHours; }
            set { _currentPaidLeaveRemainingHours = value; }
        }

        /// <summary>
        /// 当前剩余的带薪假期时间
        /// </summary>
        private double _paidLeaveRemainingHours;

        public double PaidLeaveRemainingHours
        {
            get { return _paidLeaveRemainingHours; }
            set { _paidLeaveRemainingHours = value; }
        }
        #endregion
    }
}
