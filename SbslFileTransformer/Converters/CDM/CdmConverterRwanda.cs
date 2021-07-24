using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.CDM
{
    public class CdmConverterRwanda
    {
        public CdmConverterRwanda()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<CdmColsRwanda>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        var testValue = reader.GetValue(4)?.ToString();

                        if (string.IsNullOrEmpty(testValue) || testValue.ToLower().Contains("Account".ToLower()))
                            continue;

                        var row = new CdmColsRwanda
                        {
                            //ID
                            Col2 = reader.GetValue(2)?.ToString(),
                            //ACC
                            Col4 = reader.GetValue(4)?.ToString(),
                            //CODE
                            Col9 = reader.GetValue(9)?.ToString(),
                            //NAME
                            Col12 = reader.GetValue(12)?.ToString(),
                            //COMMENT
                            Col17 = reader.GetValue(17)?.ToString(),
                            //CODE2
                            Col21 = reader.GetValue(21)?.ToString(),
                            //STATUS
                            Col23 = reader.GetValue(23)?.ToString(),
                            //CURRENCY
                            Col25 = reader.GetValue(25)?.ToString(),
                            //AMOUNT
                            Col28 = reader.GetValue(28)?.ToString(),
                            //TXN REF
                            Col33 = reader.GetValue(33)?.ToString(),
                            //DATE
                            Col40 = reader.GetValue(40)?.ToString()
                        };

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
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{fileName.Substring(Math.Max(0, fileName.Length - 10))}_IMRW.csv");
            }

            WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<CdmColsRwanda> rows, string outputFile)
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