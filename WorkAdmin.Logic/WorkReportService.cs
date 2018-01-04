using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using WorkAdmin.Models.Entities;
using System.Data.SqlClient;
using WorkAdmin.Models.ViewModels;
using WorkAdmin.Models;

namespace WorkAdmin.Logic
{
    public class WorkReportService
    {
        public static int ClearWorkReports(int selectedYear, int selectedMonth)
        {
            using (MyDbContext db = new MyDbContext())
            {
                string sql = @"
delete from dbo.workreports
where datepart(year,asofdate)=@year and datepart(month,asofdate)=@month

delete from dbo.workreportproperties
where year=@year and month=@month
";
                SqlParameter parYear = new SqlParameter("@year", selectedYear);
                SqlParameter parMonth = new SqlParameter("@month", selectedMonth);
                db.Database.ExecuteSqlCommand(sql, parYear, parMonth);
                return db.SaveChanges();
            }
        }

        public static int SaveWorkReports(IEnumerable<WorkReport> workReports)
        {
            using (MyDbContext db = new MyDbContext())
            {
                db.WorkReports.AddRange(workReports);
                return db.SaveChanges();
            }
        }

        public static WorkReportViewModel GetAllUserWorkReports(int year, int month,string sortField=null,ESortDirection sortDirection=ESortDirection.Ascending,string filterUser=null)
        {
            List<WorkReport> workReports = null;
            using (MyDbContext db = new MyDbContext())
            {
                var query = db.WorkReports.Include("User").Where(r => r.AsOfDate.Year == year && r.AsOfDate.Month == month);
                if (!string.IsNullOrWhiteSpace(filterUser))
                    query = query.Where(r =>r.User.ChineseName.Contains(filterUser)|| r.User.EnglishName.Contains(filterUser) || r.User.FullName.Contains(filterUser));
                workReports = query.ToList();
            }
            var workReportsNormal = workReports.Where(r => !r.User.IsWorkingAtHome);
            var workReportsAtHome = workReports.Where(r => r.User.IsWorkingAtHome);
            DataTable dtNormal = BuildWorkReportDataTable(workReportsNormal, year, month);
            DataTable dtAtHome = BuildWorkReportDataTable(workReportsAtHome, year, month);
            DataView dvNormal = dtNormal.DefaultView;
            if (!string.IsNullOrWhiteSpace(sortField))
                dvNormal.Sort = sortField + " " + sortDirection.GetDescription();
            return new WorkReportViewModel
            {
                Month = new DateTime(year, month, 1).ToString("MM/yyyy"),
                SortField=sortField,
                SortDirection=sortDirection,
                Filter=filterUser,
                WorkReportDataView=dvNormal,
                WorkReportAtHomeDataView=dtAtHome.DefaultView,
                UploadProperty = WorkReportPropertyService.GetWorkReportProperty(year, month),
                UserAutoCompletionSource=UserService.GetUserAutoCompletionSourceData()
            };
        }

        public static WorkReportViewModel GetWorkReports(int userId, int year, int month)
        {
            List<WorkReport> workReports = null;
            using (MyDbContext db = new MyDbContext())
            {
                workReports = db.WorkReports.Include("User").Where(r => r.UserId == userId && r.AsOfDate.Year == year && r.AsOfDate.Month == month).ToList();
            }
            var workReportsNormal = workReports.Where(r => !r.User.IsWorkingAtHome);
            var workReportsAtHome = workReports.Where(r => r.User.IsWorkingAtHome);
            DataTable dtNormal = BuildWorkReportDataTable(workReportsNormal, year, month);
            DataTable dtAtHome = BuildWorkReportDataTable(workReportsAtHome, year, month);
            return new WorkReportViewModel
            {
               Month = new DateTime(year, month, 1).ToString("MM/yyyy"),
               UploadProperty=WorkReportPropertyService.GetWorkReportProperty(year,month),
               WorkReportDataView=dtNormal.DefaultView,
               WorkReportAtHomeDataView=dtAtHome.DefaultView
            };
        }

        private static DataTable BuildWorkReportDataTable(IEnumerable<WorkReport> workReports, int year, int month)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("UserType", typeof(string));
            dt.Columns.Add("姓名", typeof(string));
            dt.Columns.Add("人员性质", typeof(string));
            dt.Columns.Add("未按时发", typeof(int));
            Dictionary<DateTime, string> dicDate = new Dictionary<DateTime, string>();
            DateTime beginDate = new DateTime(year, month, 1);
            DateTime endDate;
            if (workReports.Count() > 0)
                endDate = workReports.Max(r => r.AsOfDate);
            else
                endDate = beginDate.AddMonths(1).AddDays(-1);
            if (DateTime.Now.Date < endDate)
                endDate = DateTime.Now.Date;
            for (DateTime date = beginDate;date <= endDate; date=date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday||date.DayOfWeek == DayOfWeek.Sunday)
                    continue;
                string textDate = date.Month + "." + date.Day;
                dt.Columns.Add(textDate+"Morning", typeof(bool)).DefaultValue = false;
                dt.Columns.Add(textDate + "Noon", typeof(bool)).DefaultValue = false;
                dt.Columns.Add(textDate + "Afternoon", typeof(bool));
                dt.Columns.Add(textDate + "Evening", typeof(bool)).DefaultValue = false;
                dicDate.Add(date, textDate);
            }
            var grpWorkLogs = workReports.GroupBy(r => new { r.UserId,r.UserType}).Select(r => new { r.Key, Data = r });

            foreach (var user in grpWorkLogs)
            {
                DataRow row = dt.NewRow();
                var first = user.Data.First();
                string name = null;
                if (!string.IsNullOrWhiteSpace(first.User.ChineseName))
                    name = first.User.ChineseName;
                else if (!string.IsNullOrWhiteSpace(first.User.EnglishName))
                    name = first.User.EnglishName;
                else
                    name = first.User.FullName;

                row["UserType"] = user.Key.UserType;
                row["姓名"] = name;
                row["人员性质"] = first.EmployeeType;
                row["未按时发"] = user.Data.Select(r => (r.MorningReportAbsent ? 1 : 0) + (r.NoonReportAbsent ? 1 : 0) + (r.EveningReportAbsent ? 1 : 0)).Sum();
                foreach (var workLog in user.Data)
                {
                    if (dicDate.ContainsKey(workLog.AsOfDate))
                    {
                        row[dicDate[workLog.AsOfDate] + "Morning"] = workLog.MorningReportAbsent;
                        row[dicDate[workLog.AsOfDate] + "Noon"] = workLog.NoonReportAbsent;
                        if (workLog.AfternoonReportAbsent.HasValue)
                            row[dicDate[workLog.AsOfDate] + "Afternoon"] = workLog.AfternoonReportAbsent;
                        row[dicDate[workLog.AsOfDate] + "Evening"] = workLog.EveningReportAbsent;
                    }
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}
