using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.IO;
using System.Text;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Tanzania
{
    public class SuspenseTachBalanceExtractor
    {
        private readonly string _entity;

        public SuspenseTachBalanceExtractor(string entity)
        {
            this._entity = entity;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        internal void ConvertFile(string inputFile, string rootFolder, string outputFile = null)
        {
            ExcelCols row = new ExcelCols();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                        if (!string.IsNullOrEmpty(reader.GetValue(1)?.ToString()))
                        {
                            if (reader.GetValue(1).ToString().StartsWith("Created"))
                            {
                                //logic for row 2

                                //date
                                row.Col0 = reader.GetValue(1)?.ToString().Split(' ')[2];

                                if (!string.IsNullOrEmpty(reader.GetValue(8)?.ToString()))
                                    //currency
                                    row.Col1 = reader.GetValue(8)?.ToString().Split(':')[1];
                            }
                            else if (reader.GetValue(1).ToString().StartsWith("Net"))
                            {
                                //logic for row 21
                                if (!string.IsNullOrEmpty(reader.GetValue(10)?.ToString()))
                                    row.Col2 = reader.GetValue(10)?.ToString();
                            }
                        }
                }
            }

            this.GenerateMultiCurr(row, inputFile, rootFolder);
        }

        private void GenerateMultiCurr(ExcelCols list, string inputFile, string rootFolder)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            string outputFile = Path.Combine(rootFolder,
                $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_SUS_{this._entity}.txt");

            StringBuilder toAppend = new StringBuilder();

            DateTime date = Convert.ToDateTime(list.Col0);
            string amount = list.Col2; //vs col5 diff
            string currency = list.Col1.Trim();

            string account = "30990311001001"; //TZS

            if (currency.ToUpper() == "USD")
            {
                account = "30990411005001";
                
            }
                
            amount = (Convert.ToDouble(amount) * -1).ToString("N2");
            
            toAppend.Append(
                 $"{this._entity}\t{account}\tSuspense\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

            string text = toAppend.ToString();

            if (!string.IsNullOrEmpty(text)) File.WriteAllText(outputFile, text);
        }
    }
}