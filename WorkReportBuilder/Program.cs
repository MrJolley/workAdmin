using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkAdmin.Models.Entities;
using WorkAdmin.Logic;
using System.Configuration;
using System.Net.Mail;
using System.Net;

namespace WorkReportBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime reportDate = DateTime.MinValue;
            if (!DateTime.TryParse(ConfigurationManager.AppSettings["Date"], out reportDate))
            {
                reportDate = DateTime.Today; //统计当前月份的workreport情况
                //临时统计2月份数据
                reportDate = new DateTime(2019, 02, 01);
            }
            DoMailService dm = new DoMailService();
            //try
            //{
            var reportMail = dm.getAvailableMonthMail(reportDate.ToString("yyyyMM"));

            //获取所有需要发送report邮件的人员名单
            var users = UserService.GetAllUsers();
            var listAll = UserService.GetAllMailUsers(users);
            //var listTwo = UserService.GetTwoMailUsers(users);
            //var listSummary = UserService.GetSummaryMailUsers(users);

            //记录每一个有效工作日的report邮件
            var date = reportDate;

            while (date.Month == reportDate.Month)
            {
                try
                {
                    if (date.DayOfWeek == 0 || ((int)date.DayOfWeek == 6))
                    {
                        date = date.AddDays(1);
                        continue;
                    }
                    var availableMail = reportMail.Where(x => x.sendDate.Date == date.Date).ToList();
                    var dbReport = UserService.GetMailReportData(listAll, availableMail, reportAllType, date);
                        //.Concat(UserService.GetMailReportData(listTwo, availableMail, reportTwoType, date))
                        //.Concat(UserService.GetMailReportData(listSummary, availableMail, reportSummaryType, date)).ToList();
                    if (dbReport.Count > 0)
                    {
                        UserService.DeleteReportRecords(date);
                        UserService.SaveReportRecords(dbReport);
                    }
                    date = date.AddDays(1);
                }
                catch (Exception ex)
                {
                    dm.exceptionHandler(ex, date);
                }
            }

            Console.Write(reportDate.ToShortDateString() + "当前月份邮件读取存储已结束！");
            //}
            //catch (Exception ex)
            //{
            //    //throw;
            //    dm.exceptionHandler(ex, reportDate);
            //}
        }

        private static string[] reportAllType = new string[] { "morning", "noon", "summary" };
        private static string[] reportTwoType = new string[] { "morning", "summary" };
        private static string[] reportSummaryType = new string[] { "summary" };
    }
}
