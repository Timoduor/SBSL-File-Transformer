using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ExcelDataReader;
using SbslFileTransformer.Converters.Rwanda.BNR;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.Rwanda.FDI
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
            List<ExcelCols> list = new List<ExcelCols>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    while (reader.Read())
                    {
                        string value = reader.GetValue(0)?.ToString();
                        if (string.IsNullOrEmpty(value)) continue;
                        ExcelCols row = new ExcelCols();

                        if (reader.TryGetValue(0, out object result0)) row.Col0 = result0?.ToString().Replace("'", "");
                        if (reader.TryGetValue(1, out object result1)) row.Col1 = result1.ToString().Replace("'", "");
                        if (reader.TryGetValue(2, out object result2)) row.Col2 = result2.ToString().Replace("'", "");
                        if (reader.TryGetValue(3, out object result3)) row.Col3 = result3.ToString().Replace("'", "");
                        if (reader.TryGetValue(4, out object result4)) row.Col4 = result4.ToString().Replace("'", "");
                        if (reader.TryGetValue(5, out object result5)) row.Col5 = result5.ToString().Replace("'", "");
                        if (reader.TryGetValue(6, out object result6)) row.Col6 = result6.ToString().Replace("'", "");
                        if (reader.TryGetValue(7, out object result7)) row.Col7 = result7.ToString().Replace("'", "");
                        if (reader.TryGetValue(8, out object result8)) row.Col8 = result8.ToString().Replace("'", "");
                        if (reader.TryGetValue(9, out object result9)) row.Col9 = result9.ToString().Replace("'", "");
                        if (reader.TryGetValue(10, out object result10)) row.Col9 = result10.ToString().Replace("'", "");
                        if (reader.TryGetValue(11, out object result11)) row.Col9 = result11.ToString().Replace("'", "");
                        if (reader.TryGetValue(12, out object result12)) row.Col9 = result12.ToString().Replace("'", "");

                        list.Add(row);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);
                string fileNameToUse = fileName.Replace(" ", "").Substring(Math.Max(0, fileName.Length - 15));

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_FDI_{fileNameToUse}.csv");
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