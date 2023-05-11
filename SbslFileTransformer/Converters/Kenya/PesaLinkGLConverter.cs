using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using ExcelDataReader;

namespace SbslFileTransformer.Converters.Kenya
{
    //PesaLink_GL2
    public class PesaLinkGLConverter
    {
        public PesaLinkGLConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                ExcelReaderConfiguration config = new ExcelReaderConfiguration
                {
                    AutodetectSeparators = new char[] { ',' },
                };

                using (var reader = ExcelReaderFactory.CreateCsvReader(stream, config))
                {

                    while (reader.Read())
                    {

                        var row = new ExcelCols();

                        var check = reader.GetValue(1)?.ToString().Trim();

                        if (string.IsNullOrEmpty(check))
                        {
                            continue;
                        }

                        row.Col0 = reader.GetValue(0)?.ToString().Replace("\t", "").Trim().TrimStart();

                        row.Col1 = reader.GetValue(1)?.ToString().Replace("\t", "").Trim().TrimStart();


                        row.Col2 = reader.GetValue(2)?.ToString().Replace("\t", "").Trim();

                        row.Col3 = reader.GetValue(3)?.ToString().Replace("\t", "").Trim();

                        row.Col4 = reader.GetValue(4)?.ToString().Replace("\t", "").Trim();

                        row.Col5 = reader.GetValue(5)?.ToString().Replace("\t", "").Trim();

                        row.Col6 = reader.GetValue(6)?.ToString().Replace("\t", "").Trim();

                        row.Col7 = reader.GetValue(7)?.ToString().Replace("\t", "").Trim();

                        row.Col8 = reader.GetValue(8)?.ToString().Replace("\t", "").Trim();

                        row.Col9 = reader.GetValue(9)?.ToString().Replace("\t", "").Trim();

                        row.Col10 = reader.GetValue(10)?.ToString().Replace("\t", "").Trim();

                        row.Col11 = reader.GetValue(11)?.ToString().Replace("\t", "").Trim();

                        row.Col12 = reader.GetValue(12)?.ToString().Replace("\t", "").Trim();

                        row.Col13 = reader.GetValue(13)?.ToString().Replace("\t", "").Trim();

                        row.Col14 = reader.GetValue(14)?.ToString().Replace("\t", "").Trim();

                        row.Col15 = reader.GetValue(15)?.ToString().Replace("\t", "").Trim();

                        list.Add(row);

                    }
                }
            }

            foreach (var row in list)
            {

                if (list.Any(r => r.Col5?.Trim() == row.Col6?.Trim()))
                {
                    var rowWithCol5 = list.FirstOrDefault(r => r.Col5?.Trim() == row.Col6?.Trim());

                    if (rowWithCol5 != null && string.IsNullOrEmpty(row.Col7?.Trim()))
                    {
                        row.Col7 = rowWithCol5.Col7;
                        row.Col8 = rowWithCol5.Col8;
                        row.Col9 = rowWithCol5.Col9;
                    }
                }
            }

            foreach (var row in list)
            {
                if (string.IsNullOrEmpty(row.Col7?.Trim()))
                {
                    var rowWithValues = list.FirstOrDefault(r => r.Col6 == row.Col6 && !string.IsNullOrEmpty(r.Col7?.Trim()));

                    if (rowWithValues != null && string.IsNullOrEmpty(row.Col7?.Trim()))
                    {
                        row.Col7 = rowWithValues.Col7;
                        row.Col8 = rowWithValues.Col8;
                        row.Col9 = rowWithValues.Col9;
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm}_PesaLink_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
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
        }


    }
}
