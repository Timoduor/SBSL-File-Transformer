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

                    var phrases = line.Split("  ", StringSplitOptions.RemoveEmptyEntries);

                    column.Col1 = line.Substring(0, 4);
                    column.Col2 = line.Substring(4, 16);
                    column.Col3 = line.Substring(20, 6).Trim();

                    column.Col4 = line.Substring(26, 31);
                    column.Col5 = line.Substring(57, 2);
                    column.Col6 = line.Substring(59, 2);
                    column.Col7 = line.Substring(61, 10);
                    column.Col8 = line.Substring(71, 2);
                    column.Col9 = line.Substring(73, 3);
                    column.Col10 = line.Substring(76, 10);
                    column.Col11 = line.Substring(86, 2);
                    column.Col12 = line.Substring(88, 3);
                    column.Col13 = line.Substring(91, 25).Trim();

                    column.Col14 = line.Substring(116, 13).Trim();

                    column.Col15 = line.Substring(129, 3).Trim();
                    column.Col16 = line.Substring(132, 14).Trim();

                    column.Col17 = line.Substring(146, 3);
                    column.Col18 = line.Substring(149, 1);
                    column.Col19 = line.Substring(150, 1);
                    column.Col20 = line.Substring(151, 6);
                    column.Col21 = line.Substring(157, 2).Trim();

                    column.Col22 = line.Substring(159, 2).Trim();
                    column.Col23 = line.Substring(161, 7).Trim();

                    int month = Convert.ToInt32(column.Col5);
                    column.Col24 = Convert.ToString(month > DateTime.Now.Month ? DateTime.Now.Year - 1 : DateTime.Now.Year);

                    records.Add(column);
                }

            }


            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_Ep06_{fileName.Substring(Math.Max(0, fileName.Length - 10))}.csv");
            }

            WriteToFile(records, outputFile);
        }

        private static void WriteToFile(List<Columns> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
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
            public string? Col1 { get; set; }
            public string? Col2 { get; set; }
            public string? Col3 { get; set; }
            public string? Col4 { get; set; }
            public string? Col5 { get; set; }
            public string? Col6 { get; set; }
            public string? Col7 { get; set; }
            public string? Col8 { get; set; }
            public string? Col9 { get; set; }
            public string? Col10 { get; set; }
            public string? Col11 { get; set; }
            public string? Col12 { get; set; }
            public string? Col13 { get; set; }
            public string? Col14 { get; set; }
            public string? Col15 { get; set; }
            public string? Col16 { get; set; }
            public string? Col17 { get; set; }
            public string? Col18 { get; set; }
            public string? Col19 { get; set; }
            public string? Col20 { get; set; }
            public string? Col21 { get; set; }
            public string? Col22 { get; set; }
            public string? Col23 { get; set; }
            public string? Col24 { get; set; }
        }
    }
}
