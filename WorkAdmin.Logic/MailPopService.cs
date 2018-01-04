using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenPop.Pop3;
using OpenPop.Mime;
using System.Data.SqlClient;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using WorkAdmin.Models.Entities;

namespace WorkAdmin.Logic
{
    public class MailPopService
    {
        public MailPopService(string ac, string psd, DateTime curDate)
        {
            this.account = ac;
            this.password = psd;
            this.curDate = curDate;
        }

        public List<DailyReport> GetCurDateMail()
        {
            using (Pop3Client client = new Pop3Client())
            {
                if (client.Connected)
                {
                    client.Disconnect();
                }
                //连接263服务器
                client.Connect(host, 110, false);
                client.Authenticate(account, password);
                //读取当天邮件信息
                int count = client.GetMessageCount();
                string curShortDate = curDate.ToString("yyyyMMdd");
                List<MailProperty> lmp = new List<MailProperty>();

                for (int i = 1; i < count + 1; i++)
                {
                    Message message = client.GetMessage(i);
                    DateTime sendDate = DateTime.MinValue;
                    if (DateTime.TryParse(message.Headers.Date.Replace("(CST)", ""), out sendDate))
                    {
                        if (sendDate.ToString("yyyyMMdd").Equals(curShortDate))
                        {
                            MailProperty mp = new MailProperty();
                            mp.subject = message.Headers.Subject;
                            mp.sender = message.Headers.From.Address;
                            mp.sendDate = sendDate;
                            lmp.Add(mp);
                        }
                    }
                }
                //读取daily report邮件
                List<DailyReport> allReports = new List<DailyReport>();
                foreach (var pro in lmp)
                {
                    DailyReport report = new DailyReport();
                    if (GetMorningPlanRule(pro.sendDate, pro.subject))
                    {
                        report.type = "morning";
                        string[] rlt = morningReg.Match(pro.subject).Groups[0].Value.Split('_');
                        if (rlt.Length != 3)
                        {
                            throw new Exception("邮件标题不符合{XX_XXX_XX}的规则");
                        }
                        report.chineseName = rlt[1];
                        report.sendDate = pro.sendDate;
                        report.signIn = true;
                        allReports.Add(report);
                        continue;
                    }
                    if (GetNoonUpdateRule(pro.sendDate, pro.subject))
                    {
                        report.type = "noon";
                        string[] rlt = noonReg.Match(pro.subject).Groups[0].Value.Split('_');
                        if (rlt.Length != 3)
                        {
                            throw new Exception("邮件标题不符合{XX_XXX_XX}的规则");
                        }
                        report.chineseName = rlt[1];
                        report.sendDate = pro.sendDate;
                        report.signIn = true;
                        allReports.Add(report);
                        continue;
                    }
                    if (GetDailySummaryRule(pro.sendDate, pro.subject))
                    {
                        report.type = "summary";
                        Match ss = summaryReg.Match(pro.subject);
                        string[] rlt = summaryReg.Match(pro.subject).Groups[0].Value.Split('_');
                        if (rlt.Length != 3)
                        {
                            throw new Exception("邮件标题不符合{XX_XXX_XX}的规则");
                        }
                        report.chineseName = rlt[1];
                        report.sendDate = pro.sendDate;
                        report.signIn = true;
                        allReports.Add(report);
                        continue;
                    }
                }
                return allReports;
            }
        }

        #region Properties && Enums
        public string account { get; set; }

        public string password { get; set; }

        private string host
        {
            get { return "pop3.263.net"; }
        }

        public DateTime curDate { get; set; }

        public class MailProperty
        {
            public string subject { get; set; }

            public string sender { get; set; }

            public DateTime sendDate { get; set; }
        }

        public class DailyReport
        {
            public string type { get; set; }

            public string chineseName { get; set; }

            public DateTime sendDate { get; set; }

            public bool signIn { get; set; }
        }

        /// <summary>
        /// 邮件发送时间类型，包括早上，中午，晚上三种类型
        /// </summary>
        public enum EMailType
        {
            morning = 1,
            noon = 2,
            summary = 3,
        }
        #endregion

        #region Mail Rules Configuration
        /// <summary>
        /// 早上时间设置为9点15分，规则为此时间之前发送邮件
        /// </summary>
        /// <param name="sendingDate"></param>
        /// <returns></returns>
        public bool GetMorningPlanRule(DateTime sendingDate, string subject)
        {
            DateTime morningDate = new DateTime(curDate.Year, curDate.Month, curDate.Day, 9, 15, 0);
            return sendingDate <= morningDate && morningReg.IsMatch(subject);
        }

        /// <summary>
        /// 中午时间设置为12点整，规则为12点到1点之间发送邮件
        /// </summary>
        /// <param name="sendingDate"></param>
        /// <returns></returns>
        public bool GetNoonUpdateRule(DateTime sendingDate, string subject)
        {
            DateTime noonDate = new DateTime(curDate.Year, curDate.Month, curDate.Day, 12, 0, 0);
            return sendingDate <= noonDate.AddHours(1) && sendingDate >= noonDate
                && noonReg.IsMatch(subject);
        }

        /// <summary>
        /// 晚上时间设置为18点，规则为18点以后发送邮件
        /// </summary>
        /// <param name="sendingDate"></param>
        /// <returns></returns>
        public bool GetDailySummaryRule(DateTime sendingDate, string subject)
        {
            DateTime eveningDate = new DateTime(curDate.Year, curDate.Month, curDate.Day, 18, 0, 0);
            return sendingDate >= eveningDate && summaryReg.IsMatch(subject);
        }

        public Regex morningReg = new Regex(@"morning plan_[a-zA-Z]+\s*[a-zA-Z]+_\d{8}", RegexOptions.IgnoreCase);
        public Regex noonReg = new Regex(@"noon update_[a-zA-Z]+\s*[a-zA-Z]+_\d{8}", RegexOptions.IgnoreCase);
        public Regex summaryReg = new Regex(@"daily summary_[a-zA-Z]+\s*[a-zA-Z]+_\d{8}", RegexOptions.IgnoreCase);
        #endregion

        #region 文件下载
        public static List<WorkReportRecord> GetWorkReportRecord(DateTime dt)
        {
            using (MyDbContext db = new MyDbContext())
            {
                string sql = @"SELECT * FROM [dbo].[WorkReportRecords] WHERE year(recordDate) = @year and month(recordDate) = @month";
                return db.Database.SqlQuery<WorkReportRecord>(sql, new SqlParameter("@year", dt.Year), new SqlParameter("@month", dt.Month)).ToList();
            }
        }

        public static bool GetWorkReportExist(DateTime dt)
        {
            var data = GetWorkReportRecord(dt);
            if (data.Count > 0)
            {
                return true;
            }
            return false;
        }

        public static XSSFWorkbook GetWorkReportExcelModel(DateTime dt)
        {
            var data = GetWorkReportRecord(dt);
            var reportDate = data.AsEnumerable().Select(r => r.recordDate.Day).OrderBy(r => r).Distinct().ToList();

            var workbook = BuildExcelData(data, reportDate, dt.Year, dt.Month);
            return workbook;
        }

        public static XSSFWorkbook BuildExcelData(List<WorkReportRecord> records, List<int> reportDate, int year, int month)
        {
            SheetStylePattern styles = new SheetStylePattern();
            XSSFWorkbook workbook = styles.book;
            string sheetName = string.Format("{0}.{1}", year, month);
            ISheet sheet = workbook.CreateSheet(sheetName);
            int rowNum = 0;
            //表格备注
            SetCellValueWithStyle(sheet.CreateRow(rowNum).CreateCell(0), "统计截止时间：", styles.textStyle);
            SetCellValueWithStyle(sheet.GetRow(rowNum).CreateCell(1), sheetName, styles.textStyle);
            rowNum++;
            SetCellValueWithStyle(sheet.CreateRow(rowNum).CreateCell(0), "备注：“×” 表示没有按时发出邮件；空白表示已按时发出邮件", styles.textStyle);
            rowNum++;
            //表格时间段
            var timeRow = sheet.CreateRow(rowNum);
            for (int i = 0; i < reportDate.Count; i++)
			{
                SetCellValueWithStyle(timeRow.CreateCell(i*3 + 4), string.Format("{0}.{1}", month, reportDate[i]), styles.textStyle);
			}
            rowNum++;
            var titleRow = sheet.CreateRow(rowNum);
            SetCellValueWithStyle(titleRow.CreateCell(0), "中文名", styles.textStyle);
            SetCellValueWithStyle(titleRow.CreateCell(1), "英文名", styles.textStyle);
            SetCellValueWithStyle(titleRow.CreateCell(2), "人员性质", styles.textStyle);
            SetCellValueWithStyle(titleRow.CreateCell(3), "未按时发", styles.textStyle);
            for (int i = 0; i < reportDate.Count; i++)
            {
                SetCellValueWithStyle(titleRow.CreateCell(i * 3 + 4), "早", styles.textStyle);
                SetCellValueWithStyle(titleRow.CreateCell(i * 3 + 5), "中", styles.textStyle);
                SetCellValueWithStyle(titleRow.CreateCell(i * 3 + 6), "晚", styles.textStyle);
            }
            rowNum++;
            //员工数据段
            var allUsers = UserService.GetAllUsers();
            var listAll = UserService.GetAllMailUsers(allUsers);
            var listTwo = UserService.GetTwoMailUsers(allUsers);
            var listSummary = UserService.GetSummaryMailUsers(allUsers);
            var needMailUsers = listAll.Union(listTwo).Union(listSummary).ToList();


            var reportUsers = records.Select(r => r.userName).Distinct().ToList();
            foreach (User user in needMailUsers)
            {
                var row = sheet.CreateRow(rowNum);
                string userName = user.LetterName;
                var report = records.Where(r => r.userName.ToLower() == userName);
                if (userName.Equals(specialUser))
                {
                    report = report.Union(records.Where(r => r.userName.ToLower() == "cnabs"));
                }
                SetCellValueWithStyle(row.CreateCell(0), user.ChineseName, styles.textStyle);
                SetCellValueWithStyle(row.CreateCell(1), userName, styles.textStyle);
                SetCellValueWithStyle(row.CreateCell(2), user.rankLevel, styles.textStyle);
                SetCellValueWithStyle(row.CreateCell(3), report.Where(r => !r.signIn).Count(), styles.textStyle);
                for (int j = 0; j < reportDate.Count; j++)
                {
                    int day = reportDate[j];
                    SetCellValueWithStyle(row.CreateCell(j * 3 + 4), (report.Count() == 0 || report.Any(r => r.recordDate.Day == day && r.type.Equals("morning") && !r.signIn)) ? "×" : "", styles.textStyle);
                    SetCellValueWithStyle(row.CreateCell(j * 3 + 5), (report.Count() == 0 || report.Any(r => r.recordDate.Day == day && r.type.Equals("noon") && !r.signIn)) ? "×" : "", styles.textStyle);
                    SetCellValueWithStyle(row.CreateCell(j * 3 + 6), (report.Count() == 0 || report.Any(r => r.recordDate.Day == day && r.type.Equals("summary") && !r.signIn)) ? "×" : "", styles.textStyle);
                }
                rowNum++;
            }

            //for (int i = 0; i < count; i++)
            //{
                //var row = sheet.CreateRow(rowNum);
                //string userName = reportUsers[i];
                //var user = allUsers.Where(r => r.LetterName == userName.ToLower()).FirstOrDefault();
                //var report = records.Where(r => r.userName == userName).ToList();
                //SetCellValueWithStyle(row.CreateCell(0), user.ChineseName, styles.textStyle);
                //SetCellValueWithStyle(row.CreateCell(1), userName, styles.textStyle);
                //SetCellValueWithStyle(row.CreateCell(2), user.rankLevel, styles.textStyle);
                //(row.CreateCell(3), report.Where(r=>!r.signIn).Count(), styles.textStyle);
                //for (int j = 0; j < reportDate.Count; j++)
                //{
                //    int day = reportDate[j];
                //    SetCellValueWithStyle(row.CreateCell(j * 3 + 4), report.Any(r => r.recordDate.Day == day && r.type.Equals("morning") && !r.signIn) ? "×" : "", styles.textStyle);
                //    SetCellValueWithStyle(row.CreateCell(j * 3 + 5), report.Any(r => r.recordDate.Day == day && r.type.Equals("noon") && !r.signIn) ? "×" : "", styles.textStyle);
                //    SetCellValueWithStyle(row.CreateCell(j * 3 + 6), report.Any(r => r.recordDate.Day == day && r.type.Equals("summary") && !r.signIn) ? "×" : "", styles.textStyle);
                //}
                //rowNum++;
            //}
            sheet.DefaultColumnWidth = 10;
            return workbook;
        }

        public static ICell SetCellValueWithStyle(ICell cell, string value, ICellStyle style)
        {
            cell.CellStyle = style;
            cell.SetCellValue(value);
            return cell;
        }
        public static ICell SetCellValueWithStyle(ICell cell, int value, ICellStyle style)
        {
            cell.CellStyle = style;
            cell.SetCellValue(value);
            return cell;
        }

        public const string specialUser = "yizhi song";
        #endregion
    }
}
