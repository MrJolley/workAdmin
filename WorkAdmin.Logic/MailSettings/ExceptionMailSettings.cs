using System;
using System.Configuration;

namespace WorkAdmin.Logic.MailSettings
{
    public class ExceptionMailSettings : ConfigurationElement
    {
        [ConfigurationProperty("host")]
        public String Host
        {
            get { return (string)base["host"]; }
        }

        [ConfigurationProperty("account")]
        public String Account
        {
            get { return (string)base["account"]; }
        }

        [ConfigurationProperty("password")]
        public String Password
        {
            get { return (string)base["password"]; }
        }

        [ConfigurationProperty("fromAddress")]
        public String FromAddress
        {
            get { return (string)base["fromAddress"]; }
        }

        [ConfigurationProperty("to")]
        public String To
        {
            get { return (string)base["to"]; }
        }

        [ConfigurationProperty("subject")]
        public String Subject
        {
            get { return (string)base["subject"]; }
        }
    }
}
