using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Converters
{
    public class TzEpinConverter
    {
        public  void ConvertEPINFile(string file, string outputDirectory="")
        {
            var rowFilter = new string[] { "0500", "0700", "2500", "2700" }.ToList();//, "0600", "0620", "2500", "2700"

            var lines = File.ReadAllLines(file).ToList();

            var records = new List<EpinItem>();

            var toKeep = lines.Where(l => rowFilter.Any(e => l.StartsWith(e)));

            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.GetDirectoryName(file);
            }
            outputDirectory= Path.GetDirectoryName(file) + "\\Conv";
            //outputDirectory = Path.GetFullPath(Path.Combine(outputDirectory, @"..\")) + "Conv";
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            foreach (var line in toKeep)
            {
                var record = new EpinItem
                {
                    TransCode = line.Substring(0, 4),
                    CardNo = line.Substring(4, 16),
                    Code1 = line.Substring(20, 4),
                    RefNo= line.Substring(26, 22),
                    //RRN = line.Substring(49, 8),
                    Date = line.Substring(57, 4) + DateTime.Today.Year.ToString(),
                    TransAmount = $"{line.Substring(61, 10)}.{line.Substring(71, 2)}",
                    CurrencyCode = line.Substring(73, 3),
                    Amount = $"{line.Substring(76, 10)}.{line.Substring(86, 2)}",
                    CurrCode = line.Substring(88, 3),
                    Town = line.Substring(91, 25),
                    City = line.Substring(116, 13),
                    LocIntl = line.Substring(129, 3),
                    Code3 = line.Substring(132, 14),
                    Details = line.Substring(146, 13),
                    Code4 = line.Substring(159, 2),
                    Code5 = line.Substring(161, 7),
                };

                records.Add(record);
            }


            var outputFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(file) + ".csv");

            WriteToFile(records, outputFile);
        }

        private static void WriteToFile(List<EpinItem> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<EpinItem>();
                    csv.NextRecord();

                    foreach (var row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }


        public class EpinItem
        {
            public string TransCode { get; set; }
            public string CardNo { get; set; }
            public string Code1 { get; set; }
            //public string RRN { get; set; }
            public string RefNo { get; set; }
            public string Date { get; set; }
            public string TransAmount { get; set; }
            public string CurrencyCode { get; set; }
            public string Amount { get; set; }
            public string CurrCode { get; set; }
            public string City { get; set; }
            public string Town { get; set; }
            public string Code4 { get; set; }
            public string LocIntl { get; set; }
            public string Code3 { get; set; }
            public string Details { get; set; }
            public string Code5 { get; set; }
        }
    }
}
