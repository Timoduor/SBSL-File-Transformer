using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ExcelDataReader;

namespace SbslFileTransformer.Converters.Rwanda
{
    public class IncomingMcFinancialConverter
    {
        public IncomingMcFinancialConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();
            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        //var value = reader.GetValue(0)?.ToString();
                        //if (string.IsNullOrEmpty(value))
                        //{
                        //    continue;
                        //}
                        var row = new ExcelCols
                        {
                            //Reference
                            Col0 = reader.GetValue(0)?.ToString().Replace("\n", ""),
                            Col1 = reader.GetValue(1)?.ToString(),
                            Col2 = reader.GetValue(2)?.ToString(),
                            Col3 = reader.GetValue(3)?.ToString(),
                            Col4 = reader.GetValue(4)?.ToString(),
                            Col5 = reader.GetValue(5)?.ToString(),
                            Col6 = reader.GetValue(6)?.ToString(),
                            Col7 = reader.GetValue(7)?.ToString(),
                            Col8 = reader.GetValue(8)?.ToString(),
                            Col9 = reader.GetValue(9)?.ToString(),
                            Col10 = reader.GetValue(10)?.ToString(),
                            Col11 = reader.GetValue(11)?.ToString(),
                            Col12 = reader.GetValue(12)?.ToString(),
                            Col13 = reader.GetValue(13)?.ToString(),
                            Col14 = reader.GetValue(14)?.ToString(),
                            Col15 = reader.GetValue(15)?.ToString(),
                            Col16 = reader.GetValue(16)?.ToString(),
                            Col17 = reader.GetValue(17)?.ToString(),
                            Col18 = reader.GetValue(18)?.ToString(),
                            Col19 = reader.GetValue(19)?.ToString(),
                            Col20 = reader.GetValue(20)?.ToString(),
                            Col21 = reader.GetValue(21)?.ToString(),
                        };
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
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_McFin_{fileName.Substring(Math.Max(0, fileName.Length - 20)).Replace(" ", "")}.csv");
            }

            WriteToFile(list, outputFile);
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
            public string Col18 { get; set; }
            public string Col19 { get; set; }
            public string Col20 { get; set; }
            public string Col21 { get; set; }
            public string Col22 { get; set; }
        }
    }
}
