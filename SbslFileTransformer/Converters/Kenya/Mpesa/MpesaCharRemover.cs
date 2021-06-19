using System;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace SbslFileTransformer.Converters
{
    public class MpesaCharRemover
    {
        public void FindAndReplaceOccurrences(string inputFile, string searchText, string replaceText,
            string outputFile = null)
        {
            using (var package = new ExcelPackage(new FileInfo(inputFile)))
            {
                var sheet = package.Workbook.Worksheets.First();

                var query = from cell in sheet.Cells.Where(c => !string.IsNullOrEmpty(c.Value?.ToString()))
                    where cell.Value?.ToString()?.ToLower().Contains(searchText.ToLower()) == true
                    select cell;

                foreach (var cell in query) cell.Value = cell.Value.ToString()?.Replace(searchText, replaceText);

                if (string.IsNullOrEmpty(outputFile))
                {
                    var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                    Directory.CreateDirectory(outputFolder);

                    var fileName = Path.GetFileNameWithoutExtension(inputFile);

                    outputFile = Path.Combine(outputFolder,
                        $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{fileName.Substring(Math.Max(0, fileName.Length - 10))}.xlsx");
                }

                package.SaveAs(new FileInfo(outputFile));
            }
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            var dividend = columnNumber;
            var columnName = string.Empty;
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