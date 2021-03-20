
using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.BNR
{
    public class BnrClosingBalance
    {
        public BnrClosingBalance()
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
                    double closing = 0;
                    double opening = 0;
                    var row = new ExcelCols();
                    while (reader.Read())
                    {
                        if (reader.GetValue(15)?.ToString().Contains("Closing Balance") ?? false)
                        {
                            var val = reader.GetValue(15).ToString().Split(' ')[3];

                            closing = Convert.ToDouble(val);

                            if (closing != 0)
                            {
                                row.Col3 = closing.ToString();
                            }
                        }

                        if (reader.GetValue(9)?.ToString().Contains("Opening Balance") ?? false)
                        {
                            var val1 = reader.GetValue(9).ToString().Split(':')[1];

                            opening = Convert.ToDouble(val1);
                            if (opening != 0)
                            {
                                row.Col4 = opening.ToString();
                            }
                        }

                        if (reader.GetValue(0)?.ToString().Contains("Account:") ?? false)
                        {
                            row.Col1 = reader.GetValue(0)?.ToString().Split(new[] { ':', '-' }, StringSplitOptions.RemoveEmptyEntries)[1];
                        }
                        if (reader.GetValue(7)?.ToString().StartsWith("Date From") ?? false)
                        {
                            var data = reader.GetValue(7)?.ToString();
                            var lines = data.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var line in lines)
                            {
                                if (line.StartsWith("Currency:"))
                                {
                                    string currency = line.Split(':')[1];

                                    row.Col2 = currency.ToString();
                                }
                                else if (line.StartsWith("Date From"))
                                {
                                    var date = DateTime.ParseExact(line.Split(' ')[2], "dd-MM-yyyy", CultureInfo.InvariantCulture);
                                    row.Col0 = date.ToString();
                                }
                            }
                        }
                    }
                    //Difference between closing balance and opening balance
                    row.Col5 = (Convert.ToDouble(row.Col3) - Convert.ToDouble(row.Col4)).ToString();
                    list.Add(row);
                }
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

            //DO A MULTICURR FILE
        }
    }
}
