using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.Kenya
{
    public class WeeklyMonthlyElmaOmniSettlementConverter
    {
        public WeeklyMonthlyElmaOmniSettlementConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        internal void ConvertFile(string inputFile, string outputFile = null)
        {
            var list2 = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Use the AsDataSet extension method
                    var result = reader.AsDataSet();
                    var tables = result.Tables;

                    var sheet2 = tables[1];

                    foreach (DataRow row in sheet2.Rows)
                    {
                        var excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString().Replace("\n", "");
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();
                        excelCol.Col18 = row[18].ToString();
                        excelCol.Col19 = row[19].ToString();

                        list2.Add(excelCol);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm}_Weekly_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            WriteToFile(list2, outputFile);
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