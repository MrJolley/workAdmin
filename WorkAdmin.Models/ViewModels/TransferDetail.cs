using System;

namespace WorkAdmin.Models.ViewModels
{
    public class TransferDetail
    {
        #region constructor
        public TransferDetail()
        {
        }
        #endregion

        #region field && property
        /// <summary>
        /// 额外的工作日期，作为调休的依据
        /// </summary>
        private string _extraWorkDate;

        public string ExtraWorkDate { get => _extraWorkDate; set => _extraWorkDate = value; }

        /// <summary>
        /// 额外的工作时间段，作为调休的依据
        /// </summary>
        private string _extraWorkPeriod;

        public string ExtraWorkPeriod
        {
            get { return _extraWorkPeriod; }
            set { _extraWorkPeriod = value; }
        }

        /// <summary>
        /// 额外的工作时长
        /// </summary>
        private double _extraWorkTime;

        public double ExtraWorkTime
        {
            get { return _extraWorkTime; }
            set { _extraWorkTime = value; }
        }

        /// <summary>
        /// 调休使用的日期时间段
        /// </summary>
        private string _transferPeriod;

        public string TransferPeriod
        {
            get { return _transferPeriod; }
            set { _transferPeriod = value; }
        }

        /// <summary>
        /// 调休剩余时间
        /// </summary>
        public double TransferRemainingTime { get => _transferRemainingTime; set => _transferRemainingTime = value; }

        private double _transferRemainingTime;

        #endregion
    }
}
