using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDataReader;
using SbslFileTransformer.Converters.Rwanda.BNR;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.BalanceExtractors.Rwanda
{
    public class AirtelRwandaBalanceExtractor
    {
        private readonly string _entity;

        public AirtelRwandaBalanceExtractor(string entity)
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

                        row.Col0 = reader.GetValue(2)?.ToString().Replace("'", "");

                        row.Col1 = reader.GetValue(22)?.ToString();

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
                $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_B2W_{this._entity}.txt");

            StringBuilder toAppend = new StringBuilder();

            DateTime date = Convert.ToDateTime(list.Col0);
            string amount = list.Col1; //vs col5 diff
            string currency = "RWF";
            string account = "20100243506065";

            toAppend.Append(
                $"{this._entity}\t{account}\tMobile Banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

            string text = toAppend.ToString();

            if (!string.IsNullOrEmpty(text)) File.WriteAllText(outputFile, text);
        }
    }
}