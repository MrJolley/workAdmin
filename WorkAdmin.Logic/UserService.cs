using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using WorkAdmin.Models.Entities;
using System.Data.Entity;
using System.Web;
using WorkAdmin.Models.ViewModels;

namespace WorkAdmin.Logic
{
    public class UserService
    {
        public static Dictionary<string, int> GetUserDictionary()
        {
            var listUsers = GetAllUsers();
            Dictionary<string, int> dic = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var user in listUsers)
            {
                if (!string.IsNullOrWhiteSpace(user.FullName) && !dic.ContainsKey(user.FullName))
                    dic.Add(user.FullName, user.Id);
                if (!string.IsNullOrWhiteSpace(user.EnglishName) && !dic.ContainsKey(user.EnglishName))
                    dic.Add(user.EnglishName, user.Id);
                if (!string.IsNullOrWhiteSpace(user.ChineseName) && !dic.ContainsKey(user.ChineseName))
                    dic.Add(user.ChineseName, user.Id);
            }
            return dic;
        }

        public static UserListViewModel GetIncludedUsers(string filter = null)
        {
            List<User> users = null;
            using (MyDbContext db = new MyDbContext())
            {
                var query = db.Users.Where(r => r.IsExcluded == false);
                if (!string.IsNullOrWhiteSpace(filter))
                    query = query.Where(r => r.ChineseName.Contains(filter) || r.EnglishName.Contains(filter) || r.FullName.Contains(filter));
                users = query.OrderBy(r => r.FullName).ToList();
            }
            return new UserListViewModel
            {
                Filter = filter,
                Users = users,
                UserAutoCompletionSource = UserService.GetUserAutoCompletionSourceData()
            };
        }

        public static List<User> GetAllUsers()
        {
            using (MyDbContext db = new MyDbContext())
            {
                return db.Users.OrderBy(r => r.FullName).ToList();
            }
        }

        public static List<User> GetExcludedUsers()
        {
            using (MyDbContext db = new MyDbContext())
            {
                return db.Users.Where(r => r.IsExcluded).OrderBy(r => r.FullName).ToList();
            }
        }

        //add user to database
        public static void SyncNewUsersFromAdService(string loginName)
        {

            List<User> lstAdUsers = ADService.GetUserEntities();
            List<User> lstDbUsers = GetAllUsers();
            List<string> lstDbLoginNames = lstDbUsers.Select(r => r.LoginName.ToLower()).ToList();
            List<string> lstDcLoginNames = lstAdUsers.Select(r => r.LoginName.ToLower()).ToList();

            //delete user from database
            using (MyDbContext db = new MyDbContext())
            {
                var DbUsers = db.Users.OrderBy(r => r.FullName).ToList();
                var lstDeleteUsers = new List<User>();

                foreach (var user in DbUsers)
                {
                    if (!lstDcLoginNames.Contains(user.LoginName.ToLower()) && user.IsExcluded == true)
                    {
                        lstDeleteUsers.Add(user);
                    }
                }

                if (lstDeleteUsers.Count > 0)
                {
                    db.Users.RemoveRange(lstDeleteUsers);
                    db.SaveChanges();
                }
            }
            //add new user to database
            List<User> lstNewUsers = new List<User>();
            foreach (var user in lstAdUsers)
                if (!lstDbLoginNames.Contains(user.LoginName.ToLower()))
                {
                    user.CreatedBy = loginName;
                    user.CreatedTime = DateTime.Now;
                    user.UpdatedBy = user.CreatedBy;
                    user.UpdatedTime = user.CreatedTime;
                    lstNewUsers.Add(user);
                }
            if (lstNewUsers.Count > 0)
            {
                using (MyDbContext db = new MyDbContext())
                {
                    db.Users.AddRange(lstNewUsers);
                    db.SaveChanges();
                }
            }


            List<User> lstNewExcludedUsers = new List<User>();
            var lstAdLoginNames = lstAdUsers.Select(r => r.LoginName);
            foreach (var user in lstDbUsers.Where(r => !r.IsExcluded))
            {
                if (!lstAdLoginNames.Contains(user.LoginName, StringComparer.InvariantCultureIgnoreCase))
                {
                    user.IsExcluded = true;
                    lstNewExcludedUsers.Add(user);
                }
            }

            if (lstNewExcludedUsers.Count > 0)
            {
                using (MyDbContext db = new MyDbContext())
                {
                    foreach (var user in lstNewExcludedUsers)
                    {
                        db.Users.Attach(user);
                        db.Entry(user).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
            }
        }

        public static User GetUser(string loginName)
        {
            using (MyDbContext db = new MyDbContext())
            {
                var user = db.Users.Where(r => string.Compare(r.LoginName, loginName, true) == 0).SingleOrDefault();
                if (user == null)
                {
                    var adUser = ADService.GetUserEntity(loginName);
                    if (adUser != null)
                    {
                        adUser.CreatedBy = loginName;
                        adUser.CreatedTime = DateTime.Now;
                        adUser.UpdatedBy = adUser.CreatedBy;
                        adUser.UpdatedTime = adUser.CreatedTime;
                        db.Users.Add(adUser);
                        db.SaveChanges();
                        user = db.Users.Where(r => string.Compare(r.LoginName, loginName, true) == 0).SingleOrDefault();
                    }
                }
                return user;
            }
        }

        public static User GetUser(int id)
        {
            using (MyDbContext db = new MyDbContext())
            {
                return db.Users.Find(id);
            }
        }

        public static int UpdateUser(User user, string loginName = null)
        {
            using (MyDbContext db = new MyDbContext())
            {
                var userDb = db.Users.Find(user.Id);
                AutoMapper.Mapper.Map(user, userDb);
                if (!string.IsNullOrWhiteSpace(loginName))
                {
                    userDb.UpdatedBy = loginName;
                    userDb.UpdatedTime = DateTime.Now;
                }
                db.Users.Attach(userDb);
                db.Entry(userDb).State = EntityState.Modified;
                return db.SaveChanges();
            }
        }

        public static List<string> GetUserAutoCompletionSourceData()
        {
            List<User> listUsers = null;
            using (MyDbContext db = new MyDbContext())
            {
                listUsers = db.Users.Where(r => r.IsExcluded == false).OrderBy(r => r.EnglishName ?? r.FullName).ToList();
            }
            var listEnglish = listUsers.Where(r => !string.IsNullOrWhiteSpace(r.EnglishName)).Select(r => r.EnglishName).ToList();
            var listFull = listUsers.Select(r => r.FullName).ToList();
            var listChinese = listUsers.Where(r => !string.IsNullOrWhiteSpace(r.ChineseName)).Select(r => r.ChineseName).ToList();
            listFull.AddRange(listEnglish);
            listFull.AddRange(listChinese);
            return listFull.Distinct().ToList();
        }

        public static List<User> GetLackingChineseNameUsers()
        {
            using (MyDbContext db = new MyDbContext())
            {
                return db.Users.Where(r => r.ChineseName == null || r.ChineseName.Trim() == "").ToList();
            }
        }

        public static int FillChineseNames(List<User> users)
        {
            using (MyDbContext db = new MyDbContext())
            {
                foreach (var user in users)
                {
                    db.Users.Attach(user);
                    db.Entry(user).State = EntityState.Modified;
                }
                return db.SaveChanges();
            }
        }

        //get all user position
        public static Dictionary<string, List<string>> GetUserPositions()
        {
            Dictionary<string, List<string>> rlt = new Dictionary<string, List<string>>();
            using (MyDbContext db = new MyDbContext())
            {
                var positions = db.UserPositions;
                foreach (string category in positions.Select(r => r.category).Distinct().ToList())
                {
                    rlt.Add(category, positions.Where(r => r.category == category).Select(r => r.name).ToList());
                }
            }
            return rlt;
        }

        #region User comment
        //GAS TEAM 每天早中晚三封   
        //ABS Team 每天早晚两封 ，宋轶之早中晚三封   
        //QC Team每天早中晚三封   
        //Marketing 晚上一封 
        //注：以上只针对于analyst人员，行政人事财务、manager、developer无需发送
        #endregion

        /// <summary>
        /// 每天早中晚三封邮件
        /// GAS TEAM, QC POSITION, Product, FUND DATA, LE 每天早中晚三封
        /// GAS TEAM每天早中晚三封（更新于20180917）
        /// 宋轶之早中晚三封
        /// </summary>
        /// <param name="allUsers"></param>
        /// <returns></returns>
        public static List<User> GetAllMailUsers(List<User> allUsers)
        {
            var includedUsers = allUsers.Where(r => !r.IsExcluded && r.rankLevel != null && r.department != null && r.position != null);
            var users = (includedUsers.Where(r =>
                           r.department.Equals("GAS") && !r.rankLevel.Equals("Manager")));
                return users.ToList();
        }

        ///// <summary>
        ///// 每天早晚两封邮件
        ///// ABS DATA, Research TEAM 每天早晚两封
        ///// </summary>
        ///// <param name="allUsers"></param>
        ///// <returns></returns>
        //public static List<User> GetTwoMailUsers(List<User> allUsers)
        //{
        //    var includedUsers = allUsers.Where(r => !r.IsExcluded && r.rankLevel != null && r.department != null && r.position != null && r.project != null);
        //    var users = includedUsers.Where(r =>
        //               ((r.project.Equals("CNABS") && r.position.Equals("DATA")) || 
        //               (r.department.Equals("Research"))) 
        //        && !r.rankLevel.Equals("Manager")).ToList();
        //    users.Remove(users.Where(r => r.LetterName.Equals(specialUser)).ToList().FirstOrDefault());
        //    return users;
        //}

        ///// <summary>
        ///// 每天晚上一封邮件
        ///// Marketing 晚上一封
        ///// </summary>
        ///// <param name="allUsers"></param>
        ///// <returns></returns>
        //public static List<User> GetSummaryMailUsers(List<User> allUsers)
        //{
        //    var includedUsers = allUsers.Where(r => !r.IsExcluded && r.rankLevel != null && r.department != null && r.position != null && r.project != null);
        //    return includedUsers.Where(r =>
        //        r.department.Equals("Marketing")
        //        && !r.rankLevel.Equals("Manager")).ToList();
        //}

        public static void SaveReportRecords(List<DailyReport> report)
        {
            if (report.Count > 0)
            {
                using (MyDbContext db = new MyDbContext())
                {
                    var list = new List<WorkReportRecord>();
                    foreach (var item in report)
                    {
                        list.Add(new WorkReportRecord()
                        {
                            userName = item.chineseName,
                            type = item.type,
                            recordDate = item.sendDate,
                            signIn = item.signIn
                        });
                    }
                    var data = list.Where(x => x.signIn).ToList();
                    db.WorkReportRecords.AddRange(list);
                    db.SaveChanges();
                }
            }
        }

        public static void DeleteReportRecords(DateTime reportDate)
        {
            using (MyDbContext db = new MyDbContext())
            {
                string str = reportDate.ToString("yyyyMMdd");
                var records = db.WorkReportRecords.ToList().Where(r => r.recordDate != null && (r.recordDate.ToString("yyyyMMdd")).Equals(str));
                if (records.Count() != 0)
                {
                    db.WorkReportRecords.RemoveRange(records.ToList());
                    db.SaveChanges();
                }
            }
        }

        public static List<DailyReport> GetMailReportData(List<User> users, List<DailyReport> mailReport,
            string[] reportType, DateTime curDate)
        {
            List<DailyReport> dbReport = new List<DailyReport>();
            if (users.Count > 0)
            {
                foreach (User user in users)
                {
                    var userReport = mailReport.Where(r => string.Equals(r.chineseName.ToLower(), user.LetterName, StringComparison.CurrentCultureIgnoreCase));
                    //if (user.LetterName.Equals(specialUser))
                    //{
                    //    userReport = userReport.Union(mailReport.Where(r => r.chineseName.ToLower().Equals("cnabs")).ToList());
                    //}
                    foreach (string type in reportType)
                    {
                        if (userReport.Any(r => r.type == type))
                        {
                            dbReport.Add(userReport.Where(r => r.type == type).First());
                        }
                        else
                        {
                            dbReport.Add(new DailyReport() { chineseName = user.LetterName, type = type, sendDate = curDate, signIn = false });
                        }
                    }
                }
            }
            return dbReport;
        }

        //public const string specialUser = "yizhi song";
    }
}
