using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

using CsvHelper;

using ExcelDataReader;

using Microsoft.Extensions.Logging;

using SbslFileTransformer.Converters.Rwanda.BNR;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Tanzania;

namespace SbslFileTransformer.Converters.Tanzania
{
    public class BotWrapRemoveConverter
    {
        ILogger<BotWrapRemoverJob> _logger;
        public BotWrapRemoveConverter(ILogger<BotWrapRemoverJob> logger)
        {
            _logger = logger;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            List<ExcelCols> list = new List<ExcelCols>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(inputFile).ToLower().EndsWith(".csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {
                    // Choose one of either 1 or 2:
                    // 1. Use the reader methods

                    int rowsProcessed = 0;

                    while (reader.Read())
                    {
                        ExcelCols row = new ExcelCols();

                        if (reader.TryGetValue(0, out object col0))
                        {
                            row.Col0 = col0?.ToString();
                        }

                        if (reader.TryGetValue(1, out object col1))
                        {
                            row.Col1 = col1?.ToString();
                        }

                        if (reader.TryGetValue(2, out object col2))
                        {
                            row.Col2 = col2?.ToString();
                        }

                        if (reader.TryGetValue(3, out object col3))
                        {
                            row.Col3 = col3?.ToString();
                        }

                        if (reader.TryGetValue(4, out object col4))
                        {
                            row.Col4 = col4?.ToString();
                        }

                        if (reader.TryGetValue(5, out object col5))
                        {
                            row.Col5 = col5?.ToString().Replace('\n', ' ').Replace('\r', ' ');
                        }

                        if (reader.TryGetValue(6, out object col6))
                        {
                            row.Col6 = col6?.ToString();
                        }

                        if (reader.TryGetValue(7, out object col7))
                        {
                            row.Col7 = col7?.ToString();
                        }

                        if (reader.TryGetValue(8, out object col8))
                        {
                            row.Col8 = col8?.ToString();
                        }

                        if (reader.TryGetValue(9, out object col9))
                        {
                            row.Col9 = string.IsNullOrEmpty(col9?.ToString()) ? "0" : col9.ToString();
                        }

                        if (reader.TryGetValue(10, out object col10))
                        {
                            row.Col10 = col10?.ToString();
                        }

                        if (reader.TryGetValue(12, out object col12))
                        {
                            row.Col12 = string.IsNullOrEmpty(col12?.ToString()) ? "0" : col12.ToString();
                        }

                        if (reader.TryGetValue(11, out object col11))
                        {
                            row.Col11 = !string.IsNullOrEmpty(col12?.ToString()) && double.TryParse(col12?.ToString(), out _) ? "0" :
                                string.IsNullOrEmpty(col11?.ToString()) ? "0" : col11.ToString();
                        }

                        if (reader.TryGetValue(13, out object col13))
                        {
                            row.Col13 = col13?.ToString();
                        }

                        list.Add(row);

                        rowsProcessed++;
                    }

                    _logger.LogInformation($"Processed {rowsProcessed} rows in {stopwatch.ElapsedMilliseconds} Ms for file {inputFile.ToUpper()}");
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");

                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_BOTwrap_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            this.WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<ExcelCols> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (ExcelCols row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}
