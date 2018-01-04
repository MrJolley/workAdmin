using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace WorkReportBuilder.MailSettings
{
    public class MailConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("mailSettings")]
        public ExceptionMailSettings ExceptionMail
        {
            get { return (ExceptionMailSettings)base["mailSettings"]; }
        }
    }
}
