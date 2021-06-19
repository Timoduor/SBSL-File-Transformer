using System;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public static class DateHelper
    {
        public static bool FromExcelSerialDate(this int serialDate, out DateTime date)
        {
            try
            {
                if (serialDate > 59) serialDate -= 1; //Excel/Lotus 2/29/1900 bug
                date = new DateTime(1899, 12, 31).AddDays(serialDate);
                return true;
            }
            catch
            {
                date = default;
                return false;
            }
        }
    }
}