using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SbslFileTransformer.Converters
{
    public class SelcomBalanceExtractor
    {
        public SelcomBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFolder)
        {
            var list = new List<SelcomBalCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(inputFile).ToLower().Contains("csv"))
                {
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                }
                else if (Path.GetExtension(inputFile).ToLower().Contains("xlsx") || Path.GetExtension(inputFile).ToLower().Contains("xlsb"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else//xls
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }

                using (reader)
                {
                    // Choose one of either 1 or 2:
                    // 1. Use the reader methods

                    while (reader.Read())
                    {

                        var value = reader.GetValue(0)?.ToString();

                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                        var row = new SelcomBalCols();

                        if (DateTime.TryParseExact(reader.GetValue(0)?.ToString(), "M/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                        {
                            row.Date = resultDate;
                        }
                        else if (DateTime.TryParseExact(reader.GetValue(0)?.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                        {
                            row.Date = result;
                        }
                        else
                        {
                            continue;
                        }

                        string amount = string.IsNullOrEmpty(reader.GetValue(9)?.ToString()) ? "0" : reader.GetValue(9)?.ToString();

                        row.Amount = row.CBal = Convert.ToDouble(amount);

                        row.TransType = reader.GetValue(2)?.ToString();

                        row.Account = GetAccountNumber(inputFile);

                        list.Add(row);
                    }
                }
            }
            if (list.Count > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                var outputFile = Path.Combine(outputFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_SelcomTZ.txt");

                var lastRow = list.FirstOrDefault(c => c.Date == list.Max(r => r.Date) && (c.TransType.ToUpper() == "DEBIT" || c.TransType.ToUpper() == "CREDIT"));

                string toAppend = $"IMTZ\t{lastRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(lastRow.Date):MM/dd/yyyy}\t\t\t\t{lastRow.CBal}\tTZS\n";

                if (!string.IsNullOrEmpty(toAppend))
                {
                    File.WriteAllText(outputFile, toAppend);
                }
            }
        }

        private string GetAccountNumber(string inputFile)
        {
            if (inputFile.ToLower().Contains("b2w") && inputFile.ToLower().Contains("portal") && inputFile.ToLower().Contains("statement"))
                return "30990326501010";

            if (inputFile.ToLower().Contains("spenn") && inputFile.ToLower().Contains("selcom") && inputFile.ToLower().Contains("statement"))
                return "30990326501023";

            return "";
        }

    }


    internal class SelcomBalCols
    {
        public DateTime Date { get; set; }
        public string TransType { get; set; }
        public double Amount { get; set; }
        public string Account { get; set; }
        public double OBal { get; set; }
        public double CBal { get; set; }

    }
}
