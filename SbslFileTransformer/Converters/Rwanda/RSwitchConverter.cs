using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using ExcelDataReader;

namespace SbslFileTransformer.Converters.Rwanda
{
    public class RSwitchConverter
    {
        public RSwitchConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            int countHeader = 0;

            string time = "Time";

            string transRef = "Trans Ref";

            string deviceID = "Device ID";

            string issuer = "Issuer";

            string pan = "PAN";

            string transactiont = "Transactiont";

            string msgType = "Msg Type";

            string respCode = "Resp Code";

            string response = "Response";

            string fee = "Fee";

            string valueAmnt = "Value";

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream, new ExcelReaderConfiguration() { AutodetectSeparators = new char[] { ',', ';', '\t', '|', '#' } }))
                {
                    while (reader.Read())
                    {
                        var row = new ExcelCols();

                        if (countHeader == 0)
                        {
                            var header = new ExcelCols();

                            header.Col0 = time;

                            header.Col1 = transRef;

                            header.Col2 = deviceID;

                            header.Col3 = issuer;

                            header.Col4 = pan;

                            header.Col5 = transactiont;

                            header.Col6 = msgType;

                            header.Col7 = respCode;

                            header.Col8 = response;

                            header.Col9 = fee;

                            header.Col10 = valueAmnt;

                            list.Add(header);
                        }

                        countHeader++;

                        var value = reader.GetValue(0).ToString();

                        var value1 = reader.GetValue(8)?.ToString();

                        if (value.Contains("Time"))
                        {
                            continue;
                        }
                        if (value1 != "cashW")
                        {
                            continue;
                        }

                        row.Col0 = reader.GetValue(0)?.ToString().Trim().Replace("\n", "");

                        row.Col1 = reader.GetValue(1)?.ToString().Trim().Replace("\n", "");

                        row.Col2 = reader.GetValue(2)?.ToString().Trim().Replace("\n", "");

                        row.Col3 = reader.GetValue(4)?.ToString().Trim().Replace("\n", "");

                        row.Col4 = reader.GetValue(6)?.ToString().Trim().Replace("\n", "");

                        row.Col5 = reader.GetValue(7)?.ToString().Trim().Replace("\n", "") + reader.GetValue(8)?.ToString().Trim().Replace("\n", "") + reader.GetValue(9)?.ToString().Trim().Replace("\n", "");

                        row.Col6 = reader.GetValue(12)?.ToString().Trim().Replace("\n", "") + reader.GetValue(14)?.ToString().Trim().Replace("\n", "");

                        row.Col7 = reader.GetValue(15)?.ToString().Trim().Replace("\n", "") + reader.GetValue(16)?.ToString().Trim().Replace("\n", "");

                        row.Col8 = reader.GetValue(18)?.ToString().Trim().Replace("\n", "") + reader.GetValue(19)?.ToString().Trim().Replace("\n", "");

                        row.Col9 = reader.GetValue(20)?.ToString().Trim().Replace("\n", "") + reader.GetValue(21)?.ToString().Trim().Replace("\n", "");

                        row.Col10 = reader.GetValue(22)?.ToString().Trim().Replace("\n", "");

                        row.Col11 = reader.GetValue(25)?.ToString().Trim().Replace("\n", "");

                        //if (string.IsNullOrEmpty(value))
                        //{
                        //    continue; ;
                        //}

                        if (row.Col12 == null)
                        {
                            continue;
                        }

                        list.Add(row);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);
                var fileNameToUse = fileName.Replace(" ", "").Substring(Math.Max(0, fileName.Length - 15));

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_RSw_{fileNameToUse}.csv");
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
