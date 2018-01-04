using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WorkAdmin.Models
{
    public class Enums
    {
    }

    public enum EWorkFileType
    {
        [Description("Work Log Tracking File")]
        WorkLog,
        [Description("Work Report Tracking File")]
        WorkReport
    }

    public enum ESortDirection
    {
        [Description("asc")]
        Ascending,
        [Description("desc")]
        Desending
    }

    public enum ESearchType
    {
        [Description("pls select project")]
        SearchProject,
        [Description("pls select user")]
        SearchUser
    }
}
