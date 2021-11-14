using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Converters
{
    public class MpesaCharRemover
    {
        public void FindAndReplaceOccurrences(string inputFile, string searchText, string replaceText,
            string outputFile = null)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(inputFile)))
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets.First();

                System.Collections.Generic.IEnumerable<ExcelRangeBase> query = from cell in sheet.Cells.Where(c => !string.IsNullOrEmpty(c.Value?.ToString()))
                                                                               where cell.Value?.ToString()?.ToLower().Contains(searchText.ToLower()) == true
                                                                               select cell;

                foreach (ExcelRangeBase cell in query) cell.Value = cell.Value.ToString()?.Replace(searchText, replaceText);

                if (string.IsNullOrEmpty(outputFile))
                {
                    string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                    Directory.CreateDirectory(outputFolder);

                    string fileName = Path.GetFileNameWithoutExtension(inputFile);

                    outputFile = Path.Combine(outputFolder,
                        $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{fileName.Substring(Math.Max(0, fileName.Length - 10))}.xlsx");
                }

                package.SaveAs(new FileInfo(outputFile));
            }
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }
    }
}