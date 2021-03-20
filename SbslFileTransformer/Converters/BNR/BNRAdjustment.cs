using CsvHelper;
using ExcelDataReader;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SbslFileTransformer.Converters.BNR
{
    public class BNRAdjustment
    {
        public BNRAdjustment()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            //inputFile
            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {


                    while (reader.Read())
                    {
                        var row = new ExcelCols();

                        row.Col0 = reader.GetValue(0)?.ToString();

                        row.Col1 = reader.GetValue(1)?.ToString();

                        row.Col5 = reader.GetValue(5)?.ToString();

                        list.Add(row);
                    }
                }
            }

            var countHeader = new CountHeader
            {
                Value_date = list.First().Col0,

                Amount = list.First().Col5,

                Remittance_info = "Adjust. clearing BNR for " + list.First().Col0

            };

            //RWF
            if (list.First().Col5.Contains("-") && list.First().Col1.Contains("1240000"))
            {
                countHeader.Debit_account = "1240000";
                countHeader.DR_CR = "Debit";
                //EUR
            }
            else if (list.First().Col5.Contains("-") && list.First().Col1.Contains("1000026561"))
            {
                countHeader.Debit_account = "1000026561";
                countHeader.DR_CR = "Debit";
                //USD
            }
            else if (list.First().Col5.Contains("-") && list.First().Col1.Contains("3208000"))
            {
                countHeader.Debit_account = "3208000";
                countHeader.DR_CR = "Debit";
            }
            //RWF
            else if (list.First().Col5.Contains("") && list.First().Col1.Contains("1240000"))
            {
                countHeader.Credit_account = "1240000";
                countHeader.DR_CR = "Credit";
            }
            //EUR
            else if (list.First().Col5.Contains("") && list.First().Col1.Contains("1000026561"))
            {
                countHeader.Credit_account = "1000026561";
                countHeader.DR_CR = "Credit";
            }
            //USD
            else if (list.First().Col5.Contains("") && list.First().Col1.Contains("3208000"))
            {
                countHeader.Credit_account = "3208000";
                countHeader.DR_CR = "Credit";
            }

            WriteToFile(countHeader, outputFile);
        }

        private void WriteToFile(CountHeader row, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<CountHeader>();
                    csv.NextRecord();
                    csv.WriteRecord(row);
                }
            }
        }
    }
}
