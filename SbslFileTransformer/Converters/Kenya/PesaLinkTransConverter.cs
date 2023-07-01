using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ExcelDataReader;

namespace PesaLink_Settlement_cycle
{
    public class PesaLinkTransConverter
    {
        public PesaLinkTransConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    while (reader.Read())
                    {
                        var row = new ExcelCols();


                        row.Col0 = reader.GetValue(0)?.ToString();

                        row.Col1 = reader.GetValue(1)?.ToString();

                        row.Col2 = reader.GetValue(2)?.ToString();

                        row.Col3 = reader.GetValue(3)?.ToString();

                        row.Col4 = reader.GetValue(4)?.ToString();

                        row.Col5 = reader.GetValue(5)?.ToString();

                        row.Col6 = reader.GetValue(6)?.ToString();

                        row.Col7 = reader.GetValue(7)?.ToString();

                        row.Col8 = reader.GetValue(8)?.ToString();

                        row.Col9 = reader.GetValue(9)?.ToString();

                        row.Col10 = reader.GetValue(10)?.ToString();

                        row.Col11 = reader.GetValue(11)?.ToString();

                        row.Col12 = reader.GetValue(12)?.ToString();

                        row.Col13 = reader.GetValue(13)?.ToString();

                        row.Col14 = reader.GetValue(14)?.ToString();

                        row.Col15 = reader.GetValue(15)?.ToString();

                        row.Col16 = reader.GetValue(16)?.ToString();

                        row.Col17 = reader.GetValue(17)?.ToString().Replace("\n", "\\n");

                        if (string.IsNullOrEmpty(row.Col0) && string.IsNullOrEmpty(row.Col1) &&
                           string.IsNullOrEmpty(row.Col2) && string.IsNullOrEmpty(row.Col3))
                        {
                            continue;
                        }

                        list.Add(row);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm}_PesaLinkTrans_{fileName.Substring(0, Math.Min(fileName.Length, 14)).Replace(" ", "")}.csv");
            }

            this.WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<ExcelCols> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (var row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }

        public class ExcelCols
        {
            public string Col0 { get; set; }
            public string Col1 { get; set; }
            public string Col2 { get; set; }
            public string Col3 { get; set; }
            public string Col4 { get; set; }
            public string Col5 { get; set; }
            public string Col6 { get; set; }
            public string Col7 { get; set; }
            public string Col8 { get; set; }
            public string Col9 { get; set; }
            public string Col10 { get; set; }
            public string Col11 { get; set; }
            public string Col12 { get; set; }
            public string Col13 { get; set; }
            public string Col14 { get; set; }
            public string Col15 { get; set; }
            public string Col16 { get; set; }
            public string Col17 { get; set; }
        }
    }
}
