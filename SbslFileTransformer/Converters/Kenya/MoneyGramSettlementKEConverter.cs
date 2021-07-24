using CsvHelper;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.Kenya
{
    public class MoneyGramSettlementKEConverter
    {
        private ILogger _logger;

        public MoneyGramSettlementKEConverter(ILogger logger)
        {
            _logger = logger;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var count = 0;

                    var date = "Date";

                    var countHeader = 4;

                    while (reader.Read())
                    {
                        count++;

                        var row = new ExcelCols();

                        var value = reader.GetValue(1)?.ToString();

                        if (string.IsNullOrEmpty(value)) continue;

                        if (count == 5)
                        {
                            date = reader.GetValue(1)?.ToString();
                            continue;
                        }

                        var value2 = reader.GetValue(1)?.ToString();

                        if (string.IsNullOrEmpty(value2) || value2.Contains("Net Total") ||
                            value2.Contains("Settlement Amount")) continue;

                        if (countHeader <= count) row.Col0 = date;

                        //row.Col0 = date;

                        row.Col1 = reader.GetValue(1)?.ToString().Replace("\n", "");

                        row.Col2 = reader.GetValue(5)?.ToString().Replace("\n", "") +
                                   reader.GetValue(6)?.ToString().Replace("\n", "");

                        row.Col3 = reader.GetValue(8)?.ToString().Replace("\n", "");

                        row.Col4 = reader.GetValue(10)?.ToString().Replace("\n", "");
                        ;

                        row.Col5 = reader.GetValue(11)?.ToString().Replace("\n", "");

                        row.Col6 = reader.GetValue(12)?.ToString().Replace("\n", "");

                        row.Col7 = reader.GetValue(14)?.ToString().Replace("\n", "");

                        row.Col8 = reader.GetValue(16)?.ToString().Replace("\n", "");

                        list.Add(row);
                    }
                }
            }


            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_MG_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
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