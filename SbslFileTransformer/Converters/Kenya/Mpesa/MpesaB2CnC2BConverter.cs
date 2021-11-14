using CsvHelper;
using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class MpesaB2CnC2BConverter
    {
        private readonly string _entity;

        public MpesaB2CnC2BConverter(string entity)
        {
            _entity = entity;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string rootFolder, string outputFile = null)
        {
            //Replace empties with zeros in columns 5 and 6

            List<MPesaCols> list = new List<MPesaCols>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(inputFile).ToLower().EndsWith(".csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {
                    // Choose one of either 1 or 2:
                    // 1. Use the reader methods

                    while (reader.Read())
                    {
                        string value = reader.GetValue(0)?.ToString();

                        if (string.IsNullOrEmpty(value)) continue;
                        MPesaCols row = new MPesaCols();

                        row.Col0 = reader.GetValue(0)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col1 = reader.GetValue(1)?.ToString().Replace("\n", "").Replace("\r", "").Replace("/", "-");

                        row.Col2 = reader.GetValue(2)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col3 = reader.GetValue(3)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col4 = reader.GetValue(4)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col5 = reader.GetValue(5)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col6 = reader.GetValue(6)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col7 = reader.GetValue(7)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col8 = reader.GetValue(8)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col9 = reader.GetValue(9)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col10 = reader.GetValue(10)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col11 = reader.GetValue(11)?.ToString().Replace("\n", "").Replace("\r", "");

                        row.Col12 = reader.GetValue(12)?.ToString().Replace("\n", "").Replace("\r", "");

                        if (string.IsNullOrEmpty(row.Col5?.Trim())) row.Col5 = "0";

                        if (string.IsNullOrEmpty(row.Col6?.Trim())) row.Col6 = "0";

                        list.Add(row);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");

                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm}_B2C_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            WriteToFile(list, outputFile);

            if (inputFile.ToLower().Contains("mmf") && !inputFile.ToLower().Contains("credit_receivable"))
                GenerateMultiCurr(list.Skip(6).First(), inputFile, rootFolder);
        }


        private void GenerateMultiCurr(MPesaCols item, string inputFile, string rootFolder)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            string outputFile = Path.Combine(rootFolder,
                $"MultiCurr_{DateTime.Now:yyyy_MM_dd_mm_ss}_{fileNameToAppend}_MMF_{_entity}.txt");

            StringBuilder toAppend = new StringBuilder();

            if (!DateTime.TryParseExact(item.Col1, "d-M-yyyy HH:mm:ss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime date)) throw new Exception("Unable to parse datetime!");

            string amount = (Convert.ToDouble(item.Col7) * -1).ToString("N2"); //vs col5 diff
            string currency = "KES";

            string account = "19990126507010"; //payment

            if (inputFile.ToLower().Contains(""))

                if (inputFile.ToLower().Contains("omni")) account = "19990126505016";

            if (inputFile.ToLower().Contains("elma")) account = "19990126505009";

            toAppend.Append(
                $"{_entity}\t{account}\tMobile Banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

            string text = toAppend.ToString();

            if (!string.IsNullOrEmpty(text)) File.WriteAllText(outputFile, text);
        }


        private void WriteToFile(List<MPesaCols> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (MPesaCols row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}