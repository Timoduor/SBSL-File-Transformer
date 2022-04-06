using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using ExcelDataReader;
using SbslFileTransformer.Converters.Rwanda.BNR;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.BalanceExtractors.Rwanda
{
    public class FDIBalanceExtractor
    {
        private readonly string _entity;

        public FDIBalanceExtractor(string entity)
        {
            this._entity = entity;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string rootFolder, string outputFile = null)
        {
            List<ExcelCols> list = new List<ExcelCols>();
            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    while (reader.Read())
                    {
                        string value = reader.GetValue(0)?.ToString();

                        string value1 = reader.GetValue(3)?.ToString();

                        if (value1.Contains("float"))
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                        ExcelCols row = new ExcelCols
                        {
                            Col0 = reader.GetValue(0)?.ToString().Replace("'", ""),

                            Col2 = reader.GetValue(7)?.ToString(),

                            Col9 = reader.GetValue(9)?.ToString(),
                            Col10 = reader.GetValue(10)?.ToString(),
                        };
                        list.Add(row);
                    }
                }
            }

            //logic for getting sum

            double amount = list.Skip(1).Sum(r => Convert.ToDouble(r.Col2.ToString()));

            ExcelCols sumrow = new ExcelCols
            {
                Col0 = list.Skip(1).Select(r => r.Col0).FirstOrDefault(),

                Col1 = "Commission",

                Col2 = amount.ToString(),

            };
            List<ExcelCols> list2 = new List<ExcelCols>();
            list2.Add(sumrow);

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);
                string fileNameToUse = fileName.Replace(" ", "").Substring(Math.Max(0, fileName.Length - 10));

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{fileNameToUse}_COMM.csv");
            }

            this.WriteToCommissionFile(list2, outputFile);

            this.GenerateMultiCurr(list, inputFile, rootFolder);
        }

        private void GenerateMultiCurr(List<ExcelCols> list, string inputFile, string rootFolder)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            string outputFile = Path.Combine(rootFolder,
                $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_FDI_{this._entity}.txt");

            StringBuilder toAppend = new StringBuilder();

            list = list.Skip(1).ToList();

            DateTime date = Convert.ToDateTime(list.First().Col0.Replace("'", ""));
            double amount = Convert.ToDouble(list.First().Col9) + Convert.ToDouble(list.First().Col10); //vs col5 diff
            string currency = "RWF";
            string account = "20100243506073";

            toAppend.Append(
                $"{this._entity}\t{account}\tMobile Banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount.ToString("N2")}\t{currency}\n");

            string text = toAppend.ToString();

            if (!string.IsNullOrEmpty(text)) File.WriteAllText(outputFile, text);
        }

        private void WriteToCommissionFile(List<ExcelCols> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (ExcelCols row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}