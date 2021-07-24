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
    public class SpennRwandaBalanceExtractor
    {
        private readonly string _entity;

        public SpennRwandaBalanceExtractor(string entity)
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
                    var count = 0;

                    while (reader.Read())
                    {
                        var value = reader.GetValue(0)?.ToString();
                        if (string.IsNullOrEmpty(value)) continue;

                        if (count == 0)
                        {
                            count++;
                            continue;
                        }

                        var row = new ExcelCols();

                        row.Col0 = reader.GetValue(0)?.ToString().Replace("'", "");

                        row.Col1 = reader.GetValue(2)?.ToString();

                        list.Add(row);
                    }
                }
            }

            GenerateMultiCurr(list.First(), inputFile, rootFolder);
        }

        private void GenerateMultiCurr(ExcelCols list, string inputFile, string rootFolder)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);

            var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            var outputFile = Path.Combine(rootFolder,
                $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_SPEN_{_entity}.txt");

            var toAppend = new StringBuilder();

            if (DateTime.TryParseExact(list.Col0, "M/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var date) ||
                DateTime.TryParseExact(list.Col0, "d-MMM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out date))
            {
                var amount = list.Col1; //vs col5 diff
                var currency = "RWF";
                var account = "25049787002";

                toAppend.Append(
                    $"{_entity}\t{account}\tMobile Banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

                var text = toAppend.ToString();

                if (!string.IsNullOrEmpty(text)) File.WriteAllText(outputFile, text);
            }
            else
            {
                throw new Exception($"Unable to convert datetime value {list.Col0}");
            }
        }
    }
}