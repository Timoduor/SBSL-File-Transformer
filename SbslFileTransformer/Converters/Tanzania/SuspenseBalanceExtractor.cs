using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SbslFileTransformer.Converters.Tanzania
{
    public class SuspenseBalanceExtractor
    {
        string _entity;
        public SuspenseBalanceExtractor(string entity)
        {
            _entity = entity;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        internal void ConvertFile(string inputFile, string rootFolder, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {

                    while (reader.Read())
                    {
                        var value = reader.GetValue(1)?.ToString();

                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                        var row = new ExcelCols();

                        row.Col0 = reader.GetValue(10)?.ToString() + reader.GetValue(11)?.ToString() + reader.GetValue(12)?.ToString();

                        list.Add(row);
                    }
                }
            }

            //logic to pick the last record of the excel sheet
            var lastrecord = list.Last();

            GenerateMultiCurr(lastrecord, inputFile, rootFolder);
        }

        private void GenerateMultiCurr(ExcelCols list, string inputFile, string rootFolder)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);

            var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            var outputFile = Path.Combine(rootFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_SUSP_{_entity}.txt");

            var toAppend = new StringBuilder();

            DateTime date = Convert.ToDateTime(list.Col0);
            var amount = list.Col1; //vs col5 diff
            var currency = "TZS";
            var account = "20100243506065";

            toAppend.Append($"{_entity}\t{account}\tSuspense\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

            var text = toAppend.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                File.WriteAllText(outputFile, text);
            }
        }
    }
}
