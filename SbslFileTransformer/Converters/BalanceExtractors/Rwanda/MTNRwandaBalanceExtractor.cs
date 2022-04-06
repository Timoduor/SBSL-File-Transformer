using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDataReader;
using SbslFileTransformer.Converters.Rwanda.BNR;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.BalanceExtractors.Rwanda
{
    public class MTNRwandaBalanceExtractor
    {
        private readonly string _entity;

        public MTNRwandaBalanceExtractor(string entity)
        {
            this._entity = entity;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string rootFolder)
        {
            List<ExcelCols> list = new List<ExcelCols>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    int count = 0;

                    while (reader.Read())
                    {
                        string value = reader.GetValue(0)?.ToString();
                        if (string.IsNullOrEmpty(value)) continue;

                        if (count == 0)
                        {
                            count++;
                            continue;
                        }

                        ExcelCols row = new ExcelCols();

                        row.Col0 = reader.GetValue(0)?.ToString().Replace("'", "");

                        row.Col1 = reader.GetValue(6)?.ToString();

                        list.Add(row);
                    }
                }
            }

            this.GenerateMultiCurr(list.Last(), inputFile, rootFolder);
        }

        private void GenerateMultiCurr(ExcelCols list, string inputFile, string rootFolder)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            string outputFile = Path.Combine(rootFolder,
                $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_MTN_{this._entity}.txt");

            StringBuilder toAppend = new StringBuilder();

            if (DateTime.TryParseExact(list.Col0, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime date) ||
                DateTime.TryParseExact(list.Col0, "M/d/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out date))
            {
                string amount = list.Col1; //vs col5 diff
                string currency = "RWF";
                string account = "20100243506075";

                toAppend.Append(
                    $"{this._entity}\t{account}\tMobile Banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

                string text = toAppend.ToString();

                if (!string.IsNullOrEmpty(text)) File.WriteAllText(outputFile, text);
            }
            else
            {
                throw new Exception($"Unable to convert datetime value {list.Col0}");
            }
        }
    }
}