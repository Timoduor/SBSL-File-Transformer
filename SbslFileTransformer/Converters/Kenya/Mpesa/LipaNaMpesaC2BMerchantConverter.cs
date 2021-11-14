using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Jobs.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SbslFileTransformer.Converters.Kenya
{
    public class LipaNaMpesaC2BMerchantConverter
    {
        private readonly string _entity; //might be need for balance files generated later

        public LipaNaMpesaC2BMerchantConverter(string entity)
        {
            _entity = entity;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string rootFolder, string outputFile = null)
        {
            List<MPesaCols> list = new List<MPesaCols>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream, new ExcelReaderConfiguration
                {
                    AutodetectSeparators = new[] { '\t' }
                });

                using (reader)
                {
                    // Choose one of either 1 or 2:
                    // 1. Use the reader methods

                    while (reader.Read())
                    {
                        string value = reader.GetValue(0)?.ToString();

                        if (string.IsNullOrEmpty(value)) continue;
                        MPesaCols row = new MPesaCols();

                        row.Col0 = reader.GetValue(0)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col1 = reader.GetValue(1)?.ToString().Replace("\n", "").Replace("\r", "");

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

                        if (string.IsNullOrEmpty(row.Col7)) row.Col7 = "0";

                        if (string.IsNullOrEmpty(row.Col8)) row.Col8 = "0";

                        list.Add(row);
                    }
                }

                IEnumerable<MPesaCols> maxRecs =
                    list.GroupBy(l => l.Col0)
                        .Select(x => x.First()); //last balance record for each short code MIGHT NEED TO SKIP HEADER ROW
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_C2B_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.txt");
            }

            WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<MPesaCols> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "\t"
                }))
                {
                    foreach (MPesaCols row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}