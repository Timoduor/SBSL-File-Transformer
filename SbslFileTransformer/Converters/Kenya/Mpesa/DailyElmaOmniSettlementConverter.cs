using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ExcelDataReader;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Kenya.Mpesa
{
    public class DailyElmaOmniSettlementConverter
    {
        public DailyElmaOmniSettlementConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        internal void ConvertFile(string inputFile, string outputFile = null)
        {
            List<ExcelCols> list = new List<ExcelCols>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        string value = reader.GetValue(2)?.ToString();

                        if (string.IsNullOrEmpty(value)) continue;
                        ExcelCols row = new ExcelCols();

                        row.Col0 = reader.GetValue(2)?.ToString().Replace("\n", "");

                        row.Col1 = reader.GetValue(3)?.ToString().Replace("\n", "");

                        row.Col2 = reader.GetValue(4)?.ToString().Replace("\n", "");

                        row.Col3 = reader.GetValue(5)?.ToString().Replace("\n", "");

                        row.Col4 = reader.GetValue(6)?.ToString().Replace("\n", "");

                        row.Col5 = reader.GetValue(7)?.ToString().Replace("\n", "");

                        row.Col6 = reader.GetValue(8)?.ToString().Replace("\n", "");

                        row.Col7 = reader.GetValue(9)?.ToString().Replace("\n", "");

                        row.Col8 = reader.GetValue(10)?.ToString().Replace("\n", "");

                        row.Col9 = reader.GetValue(11)?.ToString().Replace("\n", "");

                        row.Col10 = reader.GetValue(12)?.ToString().Replace("\n", "");

                        row.Col11 = reader.GetValue(13)?.ToString().Replace("\n", "");

                        row.Col12 = reader.GetValue(14)?.ToString().Replace("\n", "");

                        if (reader.FieldCount <= 15)
                        {
                            list.Add(row);

                            continue;
                        }

                        row.Col13 = reader.GetValue(15)?.ToString().Replace("\n", "");

                        row.Col14 = reader.GetValue(16)?.ToString().Replace("\n", "");

                        row.Col15 = reader.GetValue(17)?.ToString().Replace("\n", "");

                        row.Col16 = reader.GetValue(18)?.ToString().Replace("\n", "");

                        row.Col17 = reader.GetValue(19)?.ToString().Replace("\n", "");

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
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_Daily_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
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