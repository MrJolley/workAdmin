using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using WorkAdmin.Models.Entities;

namespace WorkAdmin.Logic
{
    public class ADService
    {
        public static List<User> GetUserEntities()
        {
            var lstAdUsers = new List<User>();
            DirectoryEntry rootEntry = GetRootEntry();
            DirectorySearcher searcher = new DirectorySearcher(rootEntry, "(|(objectClass=user)(objectClass=User))");

            foreach (SearchResult entry in searcher.FindAll())
            {
                string loginNameAttr = "sAMAccountName";
                string displayNameAttr = "displayName";
                if (entry.Properties[loginNameAttr].Count > 0 && entry.Properties[displayNameAttr].Count>0)
                {
                    string loginName =entry.Properties[loginNameAttr][0].ToString();
                    string fullName = entry.Properties[displayNameAttr][0].ToString();
                    string emailNamePart = string.Join(".", fullName.Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries));
                    string emailAddress =emailNamePart + "@sail-fs.com";
                    lstAdUsers.Add(new User()
                    {
                        LoginName =loginName,
                        FullName = fullName,
                        EnglishName=fullName,
                        EmailAddress=emailAddress
                    });
                }
            }
            lstAdUsers = lstAdUsers.Distinct().ToList();
            return lstAdUsers;
        }

        public static User GetUserEntity(string loginName)
        {
            List<User> lstAdUsers = GetUserEntities();
            var userEntity = lstAdUsers.Where(r => r.LoginName.ToLower() == loginName.ToLower()).SingleOrDefault();
            return userEntity;            
        }

        //create virtual directory
        private static DirectoryEntry GetRootEntry()
        {
            string path = "LDAP://10.1.1.25/OU=Shanghai Users,DC=safs,DC=com";//DC域控制器
            var dirEntry = new DirectoryEntry(path, "worklog", "Freda1112");
            return dirEntry;
        }
    }
}
