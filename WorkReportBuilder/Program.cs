using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkAdmin.Models.Entities;
using WorkAdmin.Logic;
using System.Configuration;
using System.Net.Mail;
using WorkReportBuilder.MailSettings;
using System.Net;

namespace WorkReportBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime reportDate = DateTime.MinValue;
            try
            {
                if (!DateTime.TryParse(ConfigurationManager.AppSettings["Date"], out reportDate))
                {
                    reportDate = DateTime.Today.AddDays(-1); //统计前一天的workreport情况
                }
                MailPopService mpService = new MailPopService("workreport@sail-fs.com", "Report1234", reportDate);
                var mailReport = mpService.GetCurDateMail();
                var users = UserService.GetAllUsers();

                var listAll = UserService.GetAllMailUsers(users);
                var listTwo = UserService.GetTwoMailUsers(users);
                var listSummary = UserService.GetSummaryMailUsers(users);
                var dbReport = (UserService.GetMailReportData(listAll, mailReport, reportAllType, reportDate))
                    .Concat(UserService.GetMailReportData(listTwo, mailReport, reportTwoType, reportDate))
                    .Concat(UserService.GetMailReportData(listSummary, mailReport, reportSummaryType, reportDate)).ToList();

                UserService.DeleteReportRecords(reportDate);
                UserService.SaveReportRecords(dbReport);
                Console.Write("read mail end");
            }
            catch (Exception ex)
            {
                SmtpClient sc = new SmtpClient();
                MailMessage mms = new MailMessage();
                var mailer = (ConfigurationManager.GetSection("ExceptionNotice") as MailConfigSection).ExceptionMail;
                mms.To.Add(new MailAddress(mailer.To));
                mms.From = new MailAddress(mailer.FromAddress);
                mms.Subject = mailer.Subject;
                mms.IsBodyHtml = true;
                mms.Body = string.Format(@"<p>抓取邮件日期： {0}</p>
                                               <p>异常信息： {1}</p>
                                               <p>堆栈跟踪： {2}</p>", reportDate, ex.Message, ex.StackTrace);
                sc.UseDefaultCredentials = false;
                sc.Credentials = new NetworkCredential(mailer.Account, mailer.Password);
                sc.Host = mailer.Host;
                sc.Send(mms);
            }
        }

        private static string[] reportAllType = new string[] { "morning", "noon", "summary" };
        private static string[] reportTwoType = new string[] { "morning", "summary" };
        private static string[] reportSummaryType = new string[] { "summary" };
    }
}
