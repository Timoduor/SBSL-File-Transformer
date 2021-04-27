using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.Kenya
{
    public class OmniLookupConverter
    {
        public OmniLookupConverter()
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
                        var value = reader.GetValue(1)?.ToString();

                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                        var row = new ExcelCols();

                        //Date
                        row.Col1 = reader.GetValue(1)?.ToString().Replace("\n", "");
                        //Channel ID
                        row.Col3 = reader.GetValue(3)?.ToString().Replace("\n", "");
                        //Tran Ref No
                        row.Col4 = reader.GetValue(4)?.ToString().Replace("\n", "");
                        //Account No
                        row.Col6 = reader.GetValue(6)?.ToString().Replace("\n", "");
                        //Name
                        row.Col7 = reader.GetValue(7)?.ToString().Replace("\n", "");
                        //Currency
                        row.Col8 = reader.GetValue(8)?.ToString().Replace("\n", "");
                        //Debit Amount
                        row.Col9 = reader.GetValue(9)?.ToString().Replace("\n", "");
                        //Charge Amt
                        row.Col10 = reader.GetValue(10)?.ToString().Replace("\n", "");
                        //Network ID
                        row.Col11 = reader.GetValue(11)?.ToString().Replace("\n", "");
                        //Mobile No
                        row.Col12 = reader.GetValue(12)?.ToString().Replace("\n", "");
                        //Entered ID
                        row.Col13 = reader.GetValue(13)?.ToString().Replace("\n", "");
                        //Entered Time
                        row.Col16 = reader.GetValue(16)?.ToString().Replace("\n", "");
                        //Approved Time
                        row.Col17 = reader.GetValue(17)?.ToString().Replace("\n", "");
                        //Status
                        row.Col18 = reader.GetValue(18)?.ToString().Replace("\n", "");
                        //Bank ID
                        row.Col19 = reader.GetValue(19)?.ToString().Replace("\n", "");
                        //Merchant
                        row.Col20 = reader.GetValue(20)?.ToString().Replace("\n", "");
                        //Mobile Ref No
                        row.Col21 = reader.GetValue(21)?.ToString().Replace("\n", "");

                        list.Add(row);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm}_Omni_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
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
