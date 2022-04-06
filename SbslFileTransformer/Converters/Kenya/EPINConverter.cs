using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace SbslFileTransformer.Converters.Kenya
{
    public class EpinConverter
    {
        public void ConvertFile(string inputFile, string outputFile = null)
        {
            List<string> rowFilter = new[] { "0500", "0700", "0600", "0620", "2500", "2700" }.ToList();

            List<string> lines = File.ReadAllLines(inputFile).ToList();

            List<EpinItem> records = new List<EpinItem>();

            IEnumerable<string> toKeep = lines.Where(l => rowFilter.Any(e => l.StartsWith(e)));

            foreach (string line in toKeep)
            {
                EpinItem record = new EpinItem
                {
                    TransCode = line.Substring(0, 4),
                    CardNo = line.Substring(4, 16),
                    Code1 = line.Substring(20, 4),
                    RRN = line.Substring(49, 8),
                    Date = line.Substring(57, 4),
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
                    Code5 = line.Substring(161, 7)
                };

                records.Add(record);
            }


            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_Epin_{fileName.Substring(Math.Max(0, fileName.Length - 10))}.csv");
            }

            WriteToFile(records, outputFile);
        }

        private static void WriteToFile(List<EpinItem> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<EpinItem>();
                    csv.NextRecord();

                    foreach (EpinItem row in rows)
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
            public string RRN { get; set; }
            public string Date { get; set; }
            public string TransAmount { get; set; }
            public string CurrencyCode { get; set; }
            public string CurrCode { get; set; }
            public string City { get; set; }
            public string Town { get; set; }
            public string Code4 { get; set; }
            public string LocIntl { get; set; }
            public string Code3 { get; set; }
            public string Details { get; set; }
            public string Amount { get; set; }
            public string Code5 { get; set; }
        }
    }
}