using System.Configuration;

namespace WorkAdmin.Logic.MailSettings
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
