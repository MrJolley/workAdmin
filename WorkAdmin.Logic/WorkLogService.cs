using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkAdmin.Models.Entities;
using System.Data;
using WorkAdmin.Models;
using System.Data.SqlClient;
using WorkAdmin.Models.ViewModels;

namespace WorkAdmin.Logic
{
    public class WorkLogService
    {
        public static int ClearWorkLogs(int selectedYear, int selectedMonth)
        {
            using (MyDbContext db = new MyDbContext())
            {
                string sql = @"
delete from dbo.worklogs
where datepart(year,asofdate)=@year and datepart(month,asofdate)=@month

delete from dbo.worklogproperties
where year=@year and month=@month
";
                SqlParameter parYear = new SqlParameter("@year", selectedYear);
                SqlParameter parMonth = new SqlParameter("@month", selectedMonth);
                db.Database.ExecuteSqlCommand(sql, parYear, parMonth);
                return db.SaveChanges();
            }
        }

        public static int SaveWorkLogs(IEnumerable<WorkLog> workLogs)
        {
            using (MyDbContext db = new MyDbContext())
            {
                db.WorkLogs.AddRange(workLogs);
                return db.SaveChanges();
            }
        }
        
        public static WorkLogViewModel GetAllUserWorkLogs(int year, int month,string sortField=null,ESortDirection sortDirection=ESortDirection.Ascending,string filterUser=null)
        {
            List<WorkLog> workLogs = null;
            using (MyDbContext db = new MyDbContext())
            {
                var query = db.WorkLogs.Include("User").Where(r => r.AsOfDate.Year == year && r.AsOfDate.Month == month);
                if (!string.IsNullOrWhiteSpace(filterUser))
                    query = query.Where(r => r.User.ChineseName.Contains(filterUser) || r.User.EnglishName.Contains(filterUser) || r.User.FullName.Contains(filterUser));
                workLogs = query.ToList();
            }
            DataTable dt= BuildWorkLogDataTable(workLogs, year, month);
            DataView dv = dt.DefaultView;     
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                dv.Sort = sortField + " " + sortDirection.GetDescription();
            }
            return new WorkLogViewModel
            {
                Month=new DateTime(year,month,1).ToString("MM/yyyy"),
                SortField=sortField,
                SortDirection=sortDirection,
                Filter=filterUser,
                WorkLogDataView=dv,
                UploadProperty = WorkLogPropertyService.GetWorkLogProperty(year, month),
                UserAutoCompletionSource=UserService.GetUserAutoCompletionSourceData()
            };            
        }

        public static WorkLogViewModel GetWorkLogs(int userId,int year, int month)
        {
            List<WorkLog> workLogs = null;
            using (MyDbContext db = new MyDbContext())
            {
                workLogs = db.WorkLogs.Include("User").Where(r => r.UserId == userId && r.AsOfDate.Year == year && r.AsOfDate.Month == month).ToList();
            }
            DataTable dt= BuildWorkLogDataTable(workLogs, year, month);
            return new WorkLogViewModel
            {
                Month = new DateTime(year, month, 1).ToString("MM/yyyy"),
                UploadProperty = WorkLogPropertyService.GetWorkLogProperty(year, month),
                WorkLogDataView=dt.DefaultView
            };
        }

        private static DataTable BuildWorkLogDataTable(List<WorkLog> workLogs,int year,int month)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("姓名", typeof(string));
            dt.Columns.Add("人员性质", typeof(string));
            dt.Columns.Add("未按时发", typeof(int));
            Dictionary<DateTime, string> dicDate = new Dictionary<DateTime, string>();
            DateTime beginDate = new DateTime(year, month, 1);
            DateTime endDate;
            if (workLogs.Count > 0)
                endDate = workLogs.Max(r => r.AsOfDate);
            else 
                endDate=beginDate.AddMonths(1).AddDays(-1);
            if (DateTime.Now.Date < endDate)
                endDate = DateTime.Now.Date;
            for (DateTime date = beginDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;
                string textDate = date.Month + "." + date.Day;
                dt.Columns.Add(textDate, typeof(bool)).DefaultValue=false;
                dicDate.Add(date, textDate);
            }
            var grpWorkLogs=workLogs.GroupBy(r=>new{r.UserId}).Select(r=>new{r.Key,Data=r});

            foreach (var user in grpWorkLogs)
            {
                DataRow row = dt.NewRow();
                var first = user.Data.First();
                string name=null;                
                if(!string.IsNullOrWhiteSpace(first.User.ChineseName))
                    name=first.User.ChineseName;
                else if(!string.IsNullOrWhiteSpace(first.User.EnglishName))
                    name=first.User.EnglishName;
                else 
                    name=first.User.FullName;

                row["姓名"] = name;
                row["人员性质"] = first.EmployeeType;
                row["未按时发"] = user.Data.Where(r => r.IsAbsent).Count();
                foreach (var workLog in user.Data)
                { 
                    if(dicDate.ContainsKey(workLog.AsOfDate))
                    {
                      row[dicDate[workLog.AsOfDate]]=workLog.IsAbsent;
                    }
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}
