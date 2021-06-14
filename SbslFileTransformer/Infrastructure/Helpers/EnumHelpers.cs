using System;
using System.ComponentModel;
using System.Reflection;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public class EnumHelpers
    {
        /// <summary>
        /// Get comma separted string values stored in [Description]Attribute e.g in ReportCategory (enum)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string[] GetDescriptors(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return attributes[0].Description.ToLower().Split(',');
            else
                return value.ToString().ToLower().Split(',');
        }
    }
}
