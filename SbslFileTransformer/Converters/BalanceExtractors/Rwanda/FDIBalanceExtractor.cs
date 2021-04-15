using CsvHelper;
using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SbslFileTransformer.Converters.BalanceExtractors
{
    public class FDIBalanceExtractor
    {
        string _entity;

        public FDIBalanceExtractor(string entity)
        {
            _entity = entity;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string rootFolder, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    int count = 0;

                    while (reader.Read())
                    {
                        var value = reader.GetValue(0)?.ToString();
                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        if(count == 0)
                        {
                            count++;
                            continue;
                        }

                        var row = new ExcelCols();

                        row.Col0 = reader.GetValue(0)?.ToString().Replace("'", "");

                        row.Col2 = reader.GetValue(7)?.ToString();

                        row.Col3 = (Convert.ToDouble(reader.GetValue(9)) + Convert.ToDouble(reader.GetValue(10))).ToString();


                        list.Add(row);
                    }
                }
            }

            //logic for getting sum

            double amount = list.Sum(r => Convert.ToDouble(r.Col2.ToString()));

            var sumrow = new ExcelCols
            {
                Col0 = list.Select(r => r.Col0).FirstOrDefault(),

                Col1 = "Commission",

                Col2 = amount.ToString(),

            };
            var list2 = new List<ExcelCols>();
            list2.Add(sumrow);

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);
                var fileNameToUse = fileName.Substring(Math.Max(0, fileName.Length - 10)).Replace(" ", "");

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd}_{fileNameToUse}_COMM.csv");
            }

            WriteToCommissionFile(list2, outputFile);

            GenerateMultiCurr(list, inputFile, rootFolder);
        }

        private void GenerateMultiCurr(List<ExcelCols> list, string inputFile, string rootFolder)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);

            var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            var outputFile = Path.Combine(rootFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_FDI_{_entity}.txt");

            var toAppend = new StringBuilder();

            DateTime date = Convert.ToDateTime(list.First().Col0);
            var amount = list.First().Col3; //vs col5 diff
            var currency = "RWF";
            var account = "20100243506073";

            toAppend.Append($"{_entity}\t{account}\tMobile Banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

            var text = toAppend.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                File.WriteAllText(outputFile, text);
            }
        }

        private void WriteToCommissionFile(List<ExcelCols> rows, string outputFile)
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
