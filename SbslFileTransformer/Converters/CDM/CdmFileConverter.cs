using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ExcelDataReader;

namespace SbslFileTransformer.Converters.CDM
{
    public class CdmFileConverter
    {

        public CdmFileConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            //remove all rows where column E is blank
            //remove all columns with blanks

            var list = new List<CdmCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        var testValue = reader.GetValue(5)?.ToString();

                        if (string.IsNullOrEmpty(testValue) || testValue.ToLower().Contains("Account".ToLower()))
                        {
                            continue;
                        }

                        var row = new CdmCols
                        {
                            //ID
                            Col2 = reader.GetValue(2)?.ToString(),
                            //ACC
                            Col4 = reader.GetValue(5)?.ToString(),
                            //CODE
                            Col9 = reader.GetValue(13)?.ToString(),
                            //NAME
                            Col12 = reader.GetValue(15)?.ToString(),
                            //COMMENT
                            Col17 = reader.GetValue(18)?.ToString(),
                            //CODE2
                            Col21 = reader.GetValue(25)?.ToString(),
                            //STATUS
                            Col23 = reader.GetValue(27)?.ToString(),
                            //CURRENCY
                            Col25 = reader.GetValue(30)?.ToString(),
                            //AMOUNT
                            Col28 = reader.GetValue(31)?.ToString(),
                            //TXN REF
                            Col33 = reader.GetValue(36)?.ToString(),
                            //DATE
                            Col40 = reader.GetValue(42)?.ToString(),
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

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd}_{fileName.Substring(Math.Max(0, fileName.Length - 10))}_IMKE.csv");
            }

            WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<CdmCols> rows, string outputFile)
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

        public class CdmCols
        {
            //ID
            public string Col2 { get; set; }
            //ACC
            public string Col4 { get; set; }
            //CODE
            public string Col9 { get; set; }
            //NAME
            public string Col12 { get; set; }
            //COMMENT
            public string Col17 { get; set; }
            //CODE2
            public string Col21 { get; set; }
            //STATUS
            public string Col23 { get; set; }
            //CURRENCY
            public string Col25 { get; set; }
            //AMOUNT
            public string Col28 { get; set; }
            //TXN REF
            public string Col33 { get; set; }
            //DATE
            public string Col40 { get; set; }
        }
    }
}
