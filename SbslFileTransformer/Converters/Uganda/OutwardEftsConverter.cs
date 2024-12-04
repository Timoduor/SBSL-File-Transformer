using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Uganda
{
    public class OutwardEftsConverter
    {
        public OutwardEftsConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var reader = new StreamReader(inputFile))
            {
                var readerConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null,
                };

                using (var csv = new CsvReader(reader, readerConfig))
                {
                    while (csv.Read())
                    {
                        var row = new ExcelCols();

                        row.Col0 = csv.GetField<string>(0)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');
                        row.Col1 = csv.GetField<string>(1)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');
                        row.Col2 = csv.GetField<string>(2)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');
                        row.Col3 = csv.GetField<string>(3)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');
                        row.Col4 = csv.GetField<string>(4)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');
                        row.Col5 = csv.GetField<string>(5)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');
                        row.Col6 = csv.GetField<string>(6)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');
                        row.Col7 = csv.GetField<string>(7)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');
                        row.Col8 = csv.GetField<string>(8)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');
                        row.Col9 = csv.GetField<string>(9)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');
                        row.Col10 = csv.GetField<string>(10)?.ToString().Replace("\n", "").Replace("\r", "").TrimStart('\'');

                        list.Add(row);
                    }
                }
            }


            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                _ = Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_EFTS_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
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
