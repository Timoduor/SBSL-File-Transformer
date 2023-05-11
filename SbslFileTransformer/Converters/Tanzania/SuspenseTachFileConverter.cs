using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ExcelDataReader;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Tanzania
{
    public class SuspenseTachFileConverter
    {
        public SuspenseTachFileConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            List<ExcelCols> list = new List<ExcelCols>();

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

                    while (reader.Read())
                    {
                        string value = reader.GetValue(0)?.ToString();

                        if (string.IsNullOrEmpty(value)) continue;
                        ExcelCols row = new ExcelCols();

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

                        row.Col13 = "Inward/Outward";


                        if (row.Col4?.Trim() == "IMBLTZTZ") row.Col13 = "Outward";

                        if (row.Col5?.Trim() == "IMBLTZTZ") row.Col13 = "Inward";

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
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_TACH_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
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