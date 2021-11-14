using CsvHelper;
using SbslFileTransformer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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
            List<DateTime> holidays = new List<DateTime>();
            DateTime lastBusinessDay = new DateTime();
            int i = DateTime.DaysInMonth(date.Year, date.Month);
            while (i > 0)
            {
                DateTime dtCurrent = new DateTime(date.Year, date.Month, i);
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
            List<AccountsLookup> list = new List<AccountsLookup>();

            file = string.IsNullOrEmpty(file) ? @"C:\Users\Yida\Downloads\GL_BANK_LOOKUP.csv" : file;

            using (StreamReader reader = new StreamReader(file))
            {
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    IEnumerable<AccountsLookup> records = csv.GetRecords<AccountsLookup>();

                    list.AddRange(records.ToList());
                }
            }

            return list;
        }
    }
}