using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SbslFileTransformer.Converters.BalanceExtractors
{
    public class AirtelRwandaBalanceExtractor
    {
        string _entity;
        public AirtelRwandaBalanceExtractor(string entity)
        {
            _entity = entity;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public void ConvertFile(string inputFile, string rootFolder)
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

                        if (count == 0)
                        {
                            count++;
                            continue;
                        }

                        var row = new ExcelCols();

                        row.Col0 = reader.GetValue(2)?.ToString().Replace("'", "");

                        row.Col1 = reader.GetValue(22)?.ToString();

                        list.Add(row);
                    }
                }
            }

            GenerateMultiCurr(list.Last(), inputFile, rootFolder);
        }

        private void GenerateMultiCurr(ExcelCols list, string inputFile, string rootFolder)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);

            var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            var outputFile = Path.Combine(rootFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_B2W_{_entity}.txt");

            var toAppend = new StringBuilder();

            DateTime date = Convert.ToDateTime(list.Col0);
            var amount = list.Col3; //vs col5 diff
            var currency = "RWF";

            toAppend.Append($"{_entity}\t20100243506065\tMobile Banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

            var text = toAppend.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                File.WriteAllText(outputFile, text);
            }
        }

    }
}
