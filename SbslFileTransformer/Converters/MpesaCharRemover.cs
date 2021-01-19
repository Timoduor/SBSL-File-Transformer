using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace SbslFileTransformer.Converters
{
    public class MpesaCharRemover
    {
        public void FindAndReplaceOccurrences(string inputFile, string searchText, string replaceText, string outputFile = null)
        {
            var app = new Application();
            var workbook = app.Workbooks.Open(inputFile);
            var sheet = (Worksheet)workbook.Worksheets[1];

            var matches = new HashSet<Cell>(new CellComparer());

            var range = sheet.UsedRange;

            var next = range.Find(searchText, MatchCase: false);

            if (next != null)
            {
                matches.Add(new Cell { Row = next.Row, Col = next.Column });
            }

            while (true)
            {
                next = range.FindNext(next);

                if (!matches.Add(new Cell { Row = next.Row, Col = next.Column }))
                    break;
            }

            foreach (var cell in matches)
            {
                var current = (Range)range.Cells[cell.Row, cell.Col];

                current.Value = current.Value.ToString()?.Replace(searchText, replaceText);
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Converted");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{fileName}.xlsx");
            }

            app.DisplayAlerts = false;
            workbook.SaveAs2(outputFile);
            workbook.Close(true);
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }

    public struct Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
    }

    public class CellComparer : IEqualityComparer<Cell>
    {
        public bool Equals(Cell x, Cell y)
        {
            return x.Row == y.Row && x.Col == y.Col;
        }

        public int GetHashCode([DisallowNull] Cell obj)
        {
            return obj.GetHashCode();
        }
    }
}
