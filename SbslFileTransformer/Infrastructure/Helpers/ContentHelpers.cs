using CsvHelper;
using SbslFileTransformer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public class ContentHelpers
    {
        public static DateTime GetLastDayOfTheMonth(DateTime date2)
        {
            return new DateTime(date2.Year, date2.Month, 1).AddMonths(1).AddDays(-1);
        }

        public static DateTime GetLastBusinessDayOfMonth(DateTime date)
        {
            //exclude holidays https://stackoverflow.com/questions/273048/how-to-determine-the-last-business-day-in-a-given-month
            var holidays = new List<DateTime> {/* list of observed holidays */};
            DateTime lastBusinessDay = new DateTime();
            var i = DateTime.DaysInMonth(date.Year, date.Month);
            while (i > 0)
            {
                var dtCurrent = new DateTime(date.Year, date.Month, i);
                if (dtCurrent.DayOfWeek < DayOfWeek.Saturday && dtCurrent.DayOfWeek > DayOfWeek.Sunday)
                {
                    lastBusinessDay = dtCurrent;
                    i = 0;
                }
                else
                {
                    i = i - 1;
                }
            }

            return lastBusinessDay;
        }

        public static List<AccountsLookup> GetAccountFromCsv(string file)
        {
            var list = new List<AccountsLookup>();

            file = string.IsNullOrEmpty(file) ? @"C:\Users\Yida\Downloads\GL_BANK_LOOKUP.csv" : file;

            using (var reader = new StreamReader(file))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<AccountsLookup>();

                    list.AddRange(records.ToList());
                }
            }

            return list;
        }
    }
}
