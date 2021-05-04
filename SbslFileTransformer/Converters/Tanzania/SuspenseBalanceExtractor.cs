using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.IO;
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

            var row = new ExcelCols();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        if (!string.IsNullOrEmpty(reader.GetValue(1)?.ToString()))
                        {
                            if (reader.GetValue(1).ToString().StartsWith("Created"))
                            {
                                //logic for row 2

                                //date
                                row.Col0 = reader.GetValue(1)?.ToString().Split(' ')[2];

                                if (!string.IsNullOrEmpty(reader.GetValue(8)?.ToString()))
                                {
                                    //currency
                                    row.Col1 = reader.GetValue(8)?.ToString().Split(':')[1];
                                }
                            }
                            else if (reader.GetValue(1).ToString().StartsWith("Net"))
                            {
                                //logic for row 21
                                if (!string.IsNullOrEmpty(reader.GetValue(10)?.ToString()))
                                {
                                    row.Col2 = reader.GetValue(10)?.ToString();
                                }
                            }
                        }
                    }
                }
            }

            GenerateMultiCurr(row, inputFile, rootFolder);
        }

        private void GenerateMultiCurr(ExcelCols list, string inputFile, string rootFolder)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);

            var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            var outputFile = Path.Combine(rootFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_SUS_{_entity}.txt");

            var toAppend = new StringBuilder();

            DateTime date = Convert.ToDateTime(list.Col0);
            var amount = list.Col2; //vs col5 diff
            var currency = list.Col1.Trim();

            var account = "30990411005001";//TZS

            if(currency.ToUpper() == "USD")
            {
                account = "30990411005001";
            }

            toAppend.Append($"{_entity}\t{account}\tSuspense\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

            var text = toAppend.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                File.WriteAllText(outputFile, text);
            }
        }
    }
}
