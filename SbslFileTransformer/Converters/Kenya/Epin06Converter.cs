using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System;

namespace SbslFileTransformer.Converters.Kenya
{
    public class Epin06Converter
    {
        public void ConvertFile(string inputFile, string outputFile = null)
        {
            string[] lines = File.ReadAllLines(inputFile);

            var records = new List<Columns>();

            var allowedLines = new string[] { "0500", "0700", "0600", "0620", "2500", "2700", "2600" };

            foreach (var line in lines)
            {
                var first4chars = line.Substring(0, 4);

                if (allowedLines.Contains(first4chars))
                {
                    var column = new Columns();

                    column.TrxnCode = line.Substring(0, 4);
                    column.CardNumber = line.Substring(4, 16);
                    column.Ref1 = line.Substring(20, 6).Trim();

                    column.Arn = line.Substring(26, 31);
                    column.Month = line.Substring(57, 2);
                    column.Day = line.Substring(59, 2);
                    column.TrxnAmt = line.Substring(61, 10) + "." + line.Substring(71, 2);
                    column.TrxnCurrency = line.Substring(73, 3);
                    column.SettlAmt = line.Substring(76, 10) + "." + line.Substring(86, 2);
                    column.SettlCurrency = line.Substring(88, 3);
                    column.MerchantName = line.Substring(91, 25).Trim();

                    column.Location = line.Substring(116, 13).Trim();

                    column.Xtry = line.Substring(129, 3).Trim();
                    column.AuthCode = line.Substring(132, 14).Trim();

                    column.TrxnType = line.Substring(146, 3);
                    column.Ref2 = line.Substring(149, 1);
                    column.TrxnSeparator = line.Substring(150, 1);
                    column.Ref3 = line.Substring(151, 6);
                    column.Ref4 = line.Substring(157, 2).Trim();

                    column.Ref5 = line.Substring(159, 2).Trim();
                    column.Ref6 = line.Substring(161, 7).Trim();

                    int month = Convert.ToInt32(column.Month);
                    column.TrxnYear = Convert.ToString(month > DateTime.Now.Month ? DateTime.Now.Year - 1 : DateTime.Now.Year);

                    records.Add(column);
                }

            }

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_Ep06_{fileName.Substring(0, Math.Min(25, fileName.Length))}.csv");
            }

            WriteToFile(records, outputFile);
        }

        private static void WriteToFile(List<Columns> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<Columns>();
                    csv.NextRecord();

                    foreach (Columns row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }

        private class Columns
        {
            public string TrxnCode { get; set; }
            public string CardNumber { get; set; }
            public string Ref1 { get; set; }
            public string Arn { get; set; }
            public string Month { get; set; }
            public string Day { get; set; }
            public string TrxnAmt { get; set; }
            public string TrxnCurrency { get; set; }
            public string SettlAmt { get; set; }
            public string SettlCurrency { get; set; }
            public string MerchantName { get; set; }
            public string Location { get; set; }
            public string Xtry { get; set; }
            public string AuthCode { get; set; }
            public string TrxnType { get; set; }
            public string Ref2 { get; set; }
            public string TrxnSeparator { get; set; }
            public string Ref3 { get; set; }
            public string Ref4 { get; set; }
            public string Ref5 { get; set; }
            public string Ref6 { get; set; }
            public string TrxnYear { get; set; }
        }
    }
}
