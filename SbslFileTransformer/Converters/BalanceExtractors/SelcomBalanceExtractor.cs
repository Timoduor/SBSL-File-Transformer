using ExcelDataReader;
using OfficeOpenXml;
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

            using (var package = new ExcelPackage(new FileInfo(inputFile)))
            {
                var sheet = package.Workbook.Worksheets.First();

                var table = sheet.Tables.First();

                ExcelCellAddress start = table.Address.Start;
                ExcelCellAddress end = table.Address.End;

                for (int row = start.Row; row <= end.Row; ++row)
                {
                    ExcelRange range = sheet.Cells[row, start.Column, row, end.Column];

                    var value = sheet.Cells[row, 0, row, 0]?.ToString();

                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    var selcomRow = new SelcomBalCols();

                    if (DateTime.TryParseExact(sheet.Cells[row, 0]?.ToString(), "M/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                    {
                        selcomRow.Date = resultDate;
                    }
                    else if (DateTime.TryParseExact(sheet.Cells[row, 0]?.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    {
                        selcomRow.Date = result;
                    }
                    else
                    {
                        continue;
                    }

                    string amount = string.IsNullOrEmpty(sheet.Cells[row, 9]?.ToString()) ? "0" : sheet.Cells[row, 9]?.ToString();

                    selcomRow.Amount = selcomRow.CBal = Convert.ToDouble(amount);

                    selcomRow.TransType = sheet.Cells[row, 2]?.ToString();

                    selcomRow.Account = GetAccountNumber(inputFile);

                    list.Add(selcomRow);
                }

            }

            if (list.Count > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                var outputFile = Path.Combine(outputFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_SelcomTZ.txt");

                var lastRow = list.FirstOrDefault(c => c.Date == list.Max(r => r.Date) && (c.TransType.ToUpper() == "DEBIT" || c.TransType.ToUpper() == "CREDIT" || c.TransType.ToUpper() == "CHARGE"));

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
