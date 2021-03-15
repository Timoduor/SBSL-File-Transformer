
using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;

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

            var ext = Path.GetExtension(inputFile).ToLower();

            if (ext == ".xlsb" || ext == ".xlsx" || ext == ".xls")
            {
                GetXlsxbData(inputFile, list);
            }

            if (ext == ".csv")
            {
                GetCsvBalance(inputFile, list);
            }

            if (list.Count > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                var outputFile = Path.Combine(outputFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_MB_TZ.txt");

                var lastRow = list.LastOrDefault(c => c.Date == list.Max(r => r.Date) && (c.TransType.ToUpper() == "DEBIT" || c.TransType.ToUpper() == "CREDIT" || c.TransType.ToUpper() == "CHARGE"));

                if (lastRow != null)
                {
                    string toAppend = $"IMTZ\t{lastRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(lastRow.Date):MM/dd/yyyy}\t\t\t\t{lastRow.CBal}\tTZS\n";

                    if (!string.IsNullOrEmpty(toAppend))
                    {
                        File.WriteAllText(outputFile, toAppend);
                    }
                }
            }

            if (ext != ".csv")
            {
                ConvertToCsvFile(inputFile);
            }
        }

        private void GetCsvBalance(string inputFile, List<SelcomBalCols> list)
        {
            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream);

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
        }

        private void GetXlsxbData(string inputFile, List<SelcomBalCols> list)
        {
            Excel.Application xlApp = null;
            Excel.Workbook xlWorkbook = null;
            Excel.Worksheet sheet = null;

            try
            {
                xlApp = new Excel.Application();
                xlWorkbook = xlApp.Workbooks.Open(inputFile);
                sheet = (Excel.Worksheet)xlWorkbook.Sheets[1];
                Excel.Range xlRange = sheet.UsedRange;

                int totalColumns = xlRange.Columns.Count;
                int totalRows = xlRange.Rows.Count;

                for (int row = 1; row < totalRows; row++)
                {
                    Excel.Range range = (Excel.Range)xlRange.EntireRow[row];

                    var value = (range.Cells[row, 1] as Excel.Range).Value?.ToString();

                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }
                    var selcomRow = new SelcomBalCols();

                    if (DateTime.TryParseExact((range.Cells[row, 1] as Excel.Range).Value?.ToString(), "M/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                    {
                        selcomRow.Date = resultDate;
                    }
                    else if (DateTime.TryParseExact((range.Cells[row, 1] as Excel.Range).Value?.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    {
                        selcomRow.Date = result;
                    }
                    else
                    {
                        continue;
                    }

                    string amount = string.IsNullOrEmpty((range.Cells[row, 10] as Excel.Range).Value?.ToString()) ? "0" : (range.Cells[row, 10] as Excel.Range).Value?.ToString();

                    selcomRow.Amount = selcomRow.CBal = Convert.ToDouble(amount);

                    selcomRow.TransType = (range.Cells[row, 3] as Excel.Range).Value?.ToString();

                    selcomRow.Account = GetAccountNumber(inputFile);

                    list.Add(selcomRow);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(sheet);
                xlWorkbook.Close();
                xlApp.Quit();
            }
        }

        private void ConvertToCsvFile(string inputFile, string outputFile = null)
        {
            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd}_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            Excel.Application app = null;
            Excel.Workbook wb = null;

            try
            {
                app = new Excel.Application();
                wb = app.Workbooks.Open(inputFile);
                wb.SaveAs(outputFile, Excel.XlFileFormat.xlCSVWindows);
            }
            finally
            {
                wb.Close(false);
                app.Quit();
            }
        }

        private string GetAccountNumber(string inputFile)
        {
            if (inputFile.ToLower().Contains("w2b") && inputFile.ToLower().Contains("portal") && inputFile.ToLower().Contains("statement"))
                return "30010326501012";

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
