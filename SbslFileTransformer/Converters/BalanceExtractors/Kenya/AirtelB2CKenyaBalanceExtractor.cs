using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using ExcelDataReader;

using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.BalanceExtractors.Kenya
{
    public class AirtelB2CKenyaBalanceExtractor
    {
        public AirtelB2CKenyaBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFolder)
        {
            //Replace empties with zeros in columns 5 and 6

            List<AirtelCols> list = new List<AirtelCols>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(inputFile).ToLower().Contains("csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {
                    // Choose one of either 1 or 2:
                    // 1. Use the reader methods

                    while (reader.Read())
                    {
                        if (reader.GetValue(0)?.ToString().ToLower().Contains("transaction") ?? false) continue;

                        AirtelCols row = new AirtelCols();

                        var date = reader.GetValue(2)?.ToString();

                        if (DateTime.TryParseExact(date, "dd-MM-yyyy hh:mm tt",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                            row.ReconDate = resultDate;
                        else
                            continue;

                        row.Account = "19990126507006";

                        var amount = reader.GetValue(7)?.ToString();

                        row.Amount = Convert.ToDouble(amount);

                        list.Add(row);
                    }
                }
            }

            if (list.Count > 0)
            {
                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                string outputFile = Path.Combine(outputFolder,
                    $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_B2C_AirtelKE.txt");

                AirtelCols lastRow = list.OrderByDescending(i => i.ReconDate)
                    .FirstOrDefault(c => c.ReconDate == list.Max(r => r.ReconDate));

                string toAppend =
                    $"IMKE\t{lastRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(lastRow.ReconDate):MM/dd/yyyy}\t\t\t\t{-lastRow.Amount}\tKES\n";

                if (!string.IsNullOrEmpty(toAppend)) File.WriteAllText(outputFile, toAppend);
            }
        }
    }
}
