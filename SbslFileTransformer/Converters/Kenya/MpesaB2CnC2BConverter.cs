using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class MpesaB2CnC2BConverter
    {
        public MpesaB2CnC2BConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            //Replace empties with zeros in columns 5 and 6

            var list = new List<MPesaCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(inputFile).ToLower().EndsWith(".csv"))
                {
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                }
                else
                {
                    reader = ExcelReaderFactory.CreateReader(stream);
                }

                using (reader)
                {
                    // Choose one of either 1 or 2:
                    // 1. Use the reader methods

                    while (reader.Read())
                    {

                        var value = reader.GetValue(0)?.ToString();

                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                        var row = new MPesaCols();

                        row.Col0 = reader.GetValue(0)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col1 = reader.GetValue(1)?.ToString().Replace("\n", "").Replace("\r","").Replace("/","-");

                        row.Col2 = reader.GetValue(2)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col3 = reader.GetValue(3)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col4 = reader.GetValue(4)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col5 = reader.GetValue(5)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col6 = reader.GetValue(6)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col7 = reader.GetValue(7)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col8 = reader.GetValue(8)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col9 = reader.GetValue(9)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col10 = reader.GetValue(10)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col11 = reader.GetValue(11)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col12 = reader.GetValue(12)?.ToString().Replace("\n", "").Replace("\r", "");

                        if (string.IsNullOrEmpty(row.Col5?.Trim()))
                        {
                            row.Col5 = "0";
                        }

                        if (string.IsNullOrEmpty(row.Col6?.Trim()))
                        {

                            row.Col6 = "0";
                        }

                        list.Add(row);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");

                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm}_B2C_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ","")}.csv");
            }

            WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<MPesaCols> rows, string outputFile)
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
    }

    public class MPesaCols
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

    }
}
