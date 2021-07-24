using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class MpesaBalanceExtractor
    {
        public MpesaBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFolder)
        {
            //Replace empties with zeros in columns 5 and 6

            var list = new List<MpesaBalCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(inputFile).ToLower().Contains("csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {
                    // Choose one of either 1 or 2:
                    // 1. Use the reader methods

                    while (reader.Read())
                    {
                        var value = reader.GetValue(0)?.ToString();

                        if (string.IsNullOrEmpty(value)) continue;
                        var row = new MpesaBalCols();

                        if (DateTime.TryParseExact(reader.GetValue(1)?.ToString(), "dd-MM-yyyy HH:mm",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var resultDate))
                            row.BalDate = resultDate;
                        else if (DateTime.TryParseExact(reader.GetValue(1)?.ToString(), "dd-MM-yyyy HH:mm:ss",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var resultDate2))
                            row.BalDate = resultDate2;
                        else if (DateTime.TryParseExact(reader.GetValue(1)?.ToString(), "dd/MM/yyyy HH:mm:ss",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                            row.BalDate = result;
                        else if (DateTime.TryParseExact(reader.GetValue(1)?.ToString(), "dd/MM/yyyy HH:mm",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var result2))
                            row.BalDate = result2;
                        else if (DateTime.TryParse(reader.GetValue(1)?.ToString(), out var result3))
                            row.BalDate = result3;
                        else
                            continue;

                        var amount = string.IsNullOrEmpty(reader.GetValue(7)?.ToString())
                            ? "0"
                            : reader.GetValue(7)?.ToString();

                        row.Amount = Convert.ToDouble(amount);

                        row.Account = GetAccountNumber(inputFile);

                        list.Add(row);
                    }
                }
            }

            if (list.Count > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                var outputFile = Path.Combine(outputFolder,
                    $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_MpesaKE.txt");

                var lastRow = list.OrderByDescending(i => i.BalDate)
                    .FirstOrDefault(c => c.BalDate == list.Max(r => r.BalDate));

                var toAppend =
                    $"IMKE\t{lastRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(lastRow.BalDate):MM/dd/yyyy}\t\t\t\t{-lastRow.Amount}\tKES\n";

                if (!string.IsNullOrEmpty(toAppend)) File.WriteAllText(outputFile, toAppend);
            }
        }

        /// check file path if it contains Mpesa C2B Chango    Mpesa B2C Elma      Mpesa B2C Chango      Mpesa C2B and specify account numbers
        private string GetAccountNumber(string inputFile)
        {
            if (inputFile.ToLower().Contains("mpesa") && inputFile.ToLower().Contains("c2b") &&
                inputFile.ToLower().Contains("chango"))
                return "19990126512001";

            if (inputFile.ToLower().Contains("mpesa") && inputFile.ToLower().Contains("b2c") &&
                inputFile.ToLower().Contains("elma"))
                return "19990126505010";

            if (inputFile.ToLower().Contains("mpesa") && inputFile.ToLower().Contains("b2c") &&
                inputFile.ToLower().Contains("chango"))
                return "19990126512002";

            if (inputFile.ToLower().Contains("mpesa") && inputFile.ToLower().Contains("c2b") &&
                !inputFile.ToLower().Contains("chango"))
                return "19990126507009";

            if (inputFile.ToLower().Contains("mpesa") && inputFile.ToLower().Contains("b2c") &&
                inputFile.ToLower().Contains("omni"))
                return "19990126505017";

            if (inputFile.ToLower().Contains("mpesa") && inputFile.ToLower().Contains("to") &&
                inputFile.ToLower().Contains("till"))
                return "19990126505064";

            return "";
        }
    }
}