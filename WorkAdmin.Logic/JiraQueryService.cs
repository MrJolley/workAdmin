using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkAdmin.Models.Jira;
using WorkAdmin.Models.ViewModels;
using System.IO;
using WorkAdmin.Models;
using System.Data;
using System.ComponentModel;
using System.Collections;

namespace WorkAdmin.Logic
{
    public class JiraQueryService
    {       
        public List<JiraWorkLogDetail> GetJiraWorkLogsByUser(string userName, string startDate, string lastDate)
        {
            using (var db = new JiraDbContext())
            {
                DateTime sd = DateTime.Parse(startDate);
                DateTime ldTemp = DateTime.Parse(lastDate);
                DateTime ld = ldTemp.AddDays(1);
                var worklogs = from cwd_user cu in db.cwd_user
                               where cu.display_name == userName
                               join app_user au in db.app_user
                               on cu.lower_user_name equals au.lower_user_name
                               join worklog wl in db.worklogs
                               on au.user_key equals wl.AUTHOR
                               join jiraissue jissue in db.jiraissues
                               on wl.issueid equals jissue.ID
                               join project pro in db.projects
                               on jissue.PROJECT equals pro.ID
                               where wl.UPDATED >= sd && wl.UPDATED <= ld && cu.active == 1
                               orderby wl.UPDATED, pro.pname, cu.display_name
                               select new JiraWorkLogDetail { UpdateTime = wl.UPDATED.Value.Year.ToString() + "-" + wl.UPDATED.Value.Month.ToString() + "-" + wl.UPDATED.Value.Day.ToString(), Project = pro.pname, DisplayName = cu.display_name, WorkLogBody = wl.worklogbody, TimeWorked = wl.timeworked/3600 };
                return worklogs.ToList();
            }
        }

        public List<JiraWorkLogDetail> GetJiraWorkLogsByUsers(string startDate, string lastDate, ESortDirection sortDirection)
        {
            using (var db = new JiraDbContext())
            {
                DateTime sd = DateTime.Parse(startDate);
                DateTime ldTemp = DateTime.Parse(lastDate);
                DateTime ld = ldTemp.AddDays(1);
                var workLogs = from cwd_user cu in db.cwd_user
                               join app_user au in db.app_user
                               on cu.lower_user_name equals au.lower_user_name
                               join worklog wl in db.worklogs
                               on au.user_key equals wl.AUTHOR
                               join jiraissue jissue in db.jiraissues
                               on wl.issueid equals jissue.ID
                               join project pro in db.projects
                               on jissue.PROJECT equals pro.ID
                               where wl.UPDATED >= sd && wl.UPDATED <= ld && cu.active == 1
                               orderby wl.UPDATED.Value.Day + sortDirection, pro.pname, cu.display_name
                               select new JiraWorkLogDetail { UpdateTime = wl.UPDATED.Value.Year.ToString() + "-" + wl.UPDATED.Value.Month.ToString() + "-" + wl.UPDATED.Value.Day.ToString(), Project = pro.pname, DisplayName = cu.display_name, WorkLogBody = wl.worklogbody, TimeWorked = wl.timeworked / 3600 };
                return workLogs.ToList();
            }
        }

        public List<JiraWorkLogDetail> GetJiraWorkLogsByProject(string projectName, string startDate, string lastDate)
        {
            using (var db = new JiraDbContext())
            {
                DateTime sd = DateTime.Parse(startDate);
                DateTime ldTemp = DateTime.Parse(lastDate);
                DateTime ld = ldTemp.AddDays(1);
                var workLogs = from cwd_user cu in db.cwd_user
                               join app_user au in db.app_user
                               on cu.lower_user_name equals au.lower_user_name
                               join worklog wl in db.worklogs
                               on au.user_key equals wl.AUTHOR
                               join jiraissue jissue in db.jiraissues
                               on wl.issueid equals jissue.ID
                               join project pro in db.projects
                               on jissue.PROJECT equals pro.ID
                               where wl.UPDATED >= sd && wl.UPDATED <= ld && cu.active == 1 && pro.pname == projectName
                               orderby wl.UPDATED.Value.Day, cu.display_name
                               select new JiraWorkLogDetail { UpdateTime = wl.UPDATED.Value.Year.ToString() + "-" + wl.UPDATED.Value.Month.ToString() + "-" + wl.UPDATED.Value.Day.ToString(), Project = pro.pname, DisplayName = cu.display_name, WorkLogBody = wl.worklogbody, TimeWorked = wl.timeworked / 3600 };
                return workLogs.ToList();
            }
        }

        public List<JiraWorklogTimeUsed> GetJiraWorklogTimeUsedUser(string userName, string startDate, string lastDate)
        {
            using (var db = new JiraDbContext())
            {
                DateTime sd = DateTime.Parse(startDate);
                DateTime ldTemp = DateTime.Parse(lastDate);
                DateTime ld = ldTemp.AddDays(1);
                var timeUsed = from jiraissue jissue in db.jiraissues
                               join worklog wl in db.worklogs
                               on jissue.ID equals wl.issueid
                               where wl.UPDATED >= sd && wl.UPDATED <= ld
                               join project pro in db.projects
                               on jissue.PROJECT equals pro.ID
                               join app_user au in db.app_user
                               on wl.AUTHOR equals au.user_key
                               join cwd_user cu in db.cwd_user
                               on au.lower_user_name equals cu.lower_user_name
                               where cu.active == 1 && cu.display_name == userName
                               group wl.timeworked by new { cu.display_name, pro.pname } into q
                               orderby q.Key.display_name
                               select new JiraWorklogTimeUsed { DisplayName = q.Key.display_name, Project = q.Key.pname, TimeWorked = q.Sum(m => m.Value) / 3600 };
                return timeUsed.ToList();
            }
        }

        public List<JiraWorklogTimeUsed> GetJiraWorklogTimeUsedUser(string startDate, string lastDate)
        {
            using (var db = new JiraDbContext())
            {
                DateTime sd = DateTime.Parse(startDate);
                DateTime ldTemp = DateTime.Parse(lastDate);
                DateTime ld = ldTemp.AddDays(1);
                var timeUsed = from jiraissue jissue in db.jiraissues
                               join worklog wl in db.worklogs
                               on jissue.ID equals wl.issueid
                               where wl.UPDATED >= sd && wl.UPDATED <= ld
                               join project pro in db.projects
                               on jissue.PROJECT equals pro.ID
                               join app_user au in db.app_user
                               on wl.AUTHOR equals au.user_key
                               join cwd_user cu in db.cwd_user
                               on au.lower_user_name equals cu.lower_user_name
                               where cu.active == 1
                               group wl.timeworked by new { pro.pname, cu.display_name } into q
                               orderby q.Key.display_name
                               select new JiraWorklogTimeUsed { Project = q.Key.pname, DisplayName = q.Key.display_name, TimeWorked = q.Sum(m => m.Value) / 3600 };
                return timeUsed.ToList();
            }
        }

        public List<JiraWorklogTimeUsed> GetJiraWorklogTimeUsedProject(string projectName, string startDate, string lastDate)
        {
            using (var db = new JiraDbContext())
            {
                DateTime sd = DateTime.Parse(startDate);
                DateTime ldTemp = DateTime.Parse(lastDate);
                DateTime ld = ldTemp.AddDays(1);
                var timeUsed = from jiraissue jissue in db.jiraissues
                               join worklog wl in db.worklogs
                               on jissue.ID equals wl.issueid
                               where wl.UPDATED >= sd && wl.UPDATED <= ld 
                               join project pro in db.projects
                               on jissue.PROJECT equals pro.ID
                               join app_user au in db.app_user
                               on wl.AUTHOR equals au.user_key
                               join cwd_user cu in db.cwd_user
                               on au.lower_user_name equals cu.lower_user_name
                               where cu.active == 1 && pro.pname == projectName
                               group wl.timeworked by new { cu.display_name, pro.pname} into q
                               orderby q.Key.pname
                               select new JiraWorklogTimeUsed { DisplayName = q.Key.display_name, Project = q.Key.pname, TimeWorked = q.Sum(m => m.Value)/3600 };
                return timeUsed.ToList();
            }
        }

        public List<JiraWorklogTimeUsed> GetJiraWorklogTimeUsedProject(string startDate, string lastDate)
        {
            using (var db = new JiraDbContext())
            {
                DateTime sd = DateTime.Parse(startDate);
                DateTime ldTemp = DateTime.Parse(lastDate);
                DateTime ld = ldTemp.AddDays(1);
                var timeUsed = from jiraissue jissue in db.jiraissues
                               join worklog wl in db.worklogs
                               on jissue.ID equals wl.issueid
                               where wl.UPDATED >= sd && wl.UPDATED <= ld
                               join project pro in db.projects
                               on jissue.PROJECT equals pro.ID
                               join app_user au in db.app_user
                               on wl.AUTHOR equals au.user_key
                               join cwd_user cu in db.cwd_user
                               on au.lower_user_name equals cu.lower_user_name
                               where cu.active == 1
                               group wl.timeworked by new { pro.pname, cu.display_name } into q
                               orderby q.Key.pname
                               select new JiraWorklogTimeUsed { Project = q.Key.pname, DisplayName = q.Key.display_name, TimeWorked = q.Sum(m => m.Value)/3600 };
                return timeUsed.ToList();
            }
        }

        public List<JiraWorklogMember> GetJiraWorklogMembers(string projectName, string lastDate)
        {
            using (var db = new JiraDbContext())
            {

                DateTime ldTemp = DateTime.Parse(lastDate);
                DateTime ld = ldTemp.AddDays(1);
                DateTime beginTime = ld.AddDays(-30);
                var members = from jiraissue jissue in db.jiraissues
                              where jissue.CREATED <= ld && jissue.CREATED >= beginTime
                              join project pro in db.projects
                              on jissue.PROJECT equals pro.ID
                              join app_user au in db.app_user
                              on jissue.ASSIGNEE equals au.user_key
                              join cwd_user cu in db.cwd_user
                              on au.lower_user_name equals cu.lower_user_name
                              where cu.active == 1 && pro.pname == projectName
                              orderby pro.pname
                              select new JiraWorklogMember { Project = pro.pname, DisplayName = cu.display_name };
                return members.Distinct().ToList();
            }
        }

        public List<JiraWorklogMember> GetJiraWorklogMembers(string lastDate)
        {
            using (var db = new JiraDbContext())
            {
                DateTime ldTemp = DateTime.Parse(lastDate);
                DateTime ld = ldTemp.AddDays(1);
                DateTime beginTime = ld.AddDays(-30);
                var members = from jiraissue jissue in db.jiraissues
                              where jissue.CREATED <= ld && jissue.CREATED >= beginTime
                              join project pro in db.projects
                              on jissue.PROJECT equals pro.ID
                              join app_user au in db.app_user
                              on jissue.ASSIGNEE equals au.user_key
                              join cwd_user cu in db.cwd_user
                              on au.lower_user_name equals cu.lower_user_name
                              where cu.active == 1
                              orderby pro.pname
                              select new JiraWorklogMember { Project = pro.pname, DisplayName = cu.display_name };
                return members.Distinct().ToList();
            }
        }

        public  List<JiraWorklogUsersNameViewModel> GetJiraUsersName()
        {
            using (var db = new JiraDbContext())
            {
                var users = from cwd_user cu in db.cwd_user
                            where cu.active == 1
                            select new JiraWorklogUsersNameViewModel { usersName = cu.display_name};
                return users.ToList();
            }
        }

        public  List<JiraWorklogProjectViewModel> GetJiraWorklogProject()
        {
            using (var db = new JiraDbContext())
            {
                var project = from project pro in db.projects
                              select new JiraWorklogProjectViewModel { projectId = pro.ID, projectName = pro.pname };
                return project.ToList();
            }
        }

        public static DataTable ToDataTableTow<T>(IList<T> list)
        {
            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                System.Reflection.PropertyInfo[] propertys = list[0].GetType().GetProperties();

                foreach (System.Reflection.PropertyInfo pi in propertys)
                {
                    result.Columns.Add(pi.Name, pi.PropertyType);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (System.Reflection.PropertyInfo pi in propertys)
                    {
                        object obj = pi.GetValue(list[i], null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            return result;
        }  

    }
}
