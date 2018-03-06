using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WorkAdmin.Models.Entities;
using WorkAdmin.Logic;

namespace WorkAdmin.Site
{
    public class MyHttpUtilities
    {
        private User _currentUser;
        public User CurrentUser
        {
            get
            {
                if (_currentUser == null && HttpContext.Current.User.Identity != null)
                {
                    string identityName = HttpContext.Current.User.Identity.Name;
                    if (!string.IsNullOrWhiteSpace(identityName))
                    {
                        string loginName = identityName.Substring(identityName.IndexOf(@"\") + 1);
                        _currentUser = UserService.GetUser(loginName);
                    }
                }
                return _currentUser;
            }
        }

        private string _loginName;
        public string LoginName
        {
            get
            {
                if (_loginName == null && HttpContext.Current.User.Identity != null)
                {
                    string identityName = HttpContext.Current.User.Identity.Name;
                    if (identityName != null)
                        _loginName = identityName.Substring(identityName.IndexOf(@"\") + 1);
                }
                return _loginName;
            }
        }

        private string _userEmail;
        public string UserEmail
        {
            get
            {
                if (_userEmail == null && HttpContext.Current.User.Identity != null)
                {
                    string identityName = HttpContext.Current.User.Identity.Name;
                    if (identityName != null)
                    {
                        string loginName = identityName.Substring(identityName.IndexOf(@"\") + 1);
                        _userEmail = UserService.GetUser(loginName).EmailAddress;
                    }
                }
                return _userEmail;
            }
        }
    }
}