using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace WorkAdmin.Logic
{
    public static class Extensions
    {
        public static string GetDescription(this Enum enumberation)
        {
            FieldInfo fi = enumberation.GetType().GetField(enumberation.ToString());
            var descriptionAttr = fi.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttr != null)
                return descriptionAttr.Description;
            else
                return enumberation.ToString();
        }
    }
}
