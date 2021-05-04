using CsvHelper;
using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.BalanceExtractors
{
    public class FDICleaner
    {
        public FDICleaner()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        //remove apostrophes for all folders except bal
        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    while (reader.Read())
                    {
                        var value = reader.GetValue(0)?.ToString();
                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                        var row = new ExcelCols();

                        if (reader.TryGetValue(0, out var result0))
                        {
                            row.Col0 = result0?.ToString().Replace("'", "");
                        }
                        if (reader.TryGetValue(1, out var result1))
                        {
                            row.Col1 = result1.ToString().Replace("'", "");
                        }
                        if (reader.TryGetValue(2, out var result2))
                        {
                            row.Col2 = result2.ToString().Replace("'", "");
                        }
                        if (reader.TryGetValue(3, out var result3))
                        {
                            row.Col3 = result3.ToString().Replace("'", "");
                        }
                        if (reader.TryGetValue(4, out var result4))
                        {
                            row.Col4 = result4.ToString().Replace("'", "");
                        }
                        if (reader.TryGetValue(5, out var result5))
                        {
                            row.Col5 = result5.ToString().Replace("'", "");
                        }
                        if (reader.TryGetValue(6, out var result6))
                        {
                            row.Col6 = result6.ToString().Replace("'", "");
                        }
                        if (reader.TryGetValue(7, out var result7))
                        {
                            row.Col7 = result7.ToString().Replace("'", "");
                        }
                        if (reader.TryGetValue(8, out var result8))
                        {
                            row.Col8 = result8.ToString().Replace("'", "");
                        }
                        if (reader.TryGetValue(9, out var result9))
                        {
                            row.Col9 = result9.ToString().Replace("'", "");
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
                var fileNameToUse = fileName.Replace(" ", "").Substring(Math.Max(0, fileName.Length - 15));

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_FDI_{fileNameToUse}.csv");
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
    }
}
