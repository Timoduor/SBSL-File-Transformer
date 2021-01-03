using CsvHelper;
using ExcelDataReader;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters
{
    public class CdmFileConverter
    {

        public CdmFileConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile)
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
                        if (string.IsNullOrEmpty(reader.GetValue(4)?.ToString()))
                        {
                            continue;
                        }

                        var row = new CdmCols
                        {
                            Col2 = reader.GetValue(2)?.ToString(),
                            Col4 = reader.GetValue(4)?.ToString(),
                            Col9 = reader.GetValue(9)?.ToString(),
                            Col12 = reader.GetValue(12)?.ToString(),
                            Col17 = reader.GetValue(17)?.ToString(),
                            Col21 = reader.GetValue(21)?.ToString(),
                            Col23 = reader.GetValue(23)?.ToString(),
                            Col25 = reader.GetValue(25)?.ToString(),
                            Col28 = reader.GetValue(28)?.ToString(),
                            Col33 = reader.GetValue(33)?.ToString(),
                            Col40 = reader.GetValue(40)?.ToString(),
                        };

                        list.Add(row);
                    }
                }
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
            public string Col2 { get; set; }
            public string Col4 { get; set; }
            public string Col9 { get; set; }
            public string Col12 { get; set; }
            public string Col17 { get; set; }
            public string Col21 { get; set; }
            public string Col23 { get; set; }
            public string Col25 { get; set; }
            public string Col28 { get; set; }
            public string Col33 { get; set; }
            public string Col40 { get; set; }
        }
    }
}
