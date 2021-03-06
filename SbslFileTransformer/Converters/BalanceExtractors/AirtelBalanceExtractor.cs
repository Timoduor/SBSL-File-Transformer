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
    public class AirtelBalanceExtractor
    {
        public AirtelBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFolder)
        {
            //Replace empties with zeros in columns 5 and 6

            var list = new List<AirtelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(inputFile).ToLower().Contains("csv"))
                {
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                }
                else
                {
                    reader = ExcelReaderFactory.CreateReader(stream);
                }

                using (reader)
                {
                    // Choose one of either 1 or 2:
                    // 1. Use the reader methods

                    while (reader.Read())
                    {
                        if (reader.GetValue(0)?.ToString().ToLower().Contains("transaction") ?? false)
                        {
                            continue;
                        }

                        var row = new AirtelCols();

                        if (DateTime.TryParseExact(reader.GetValue(2)?.ToString(), "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                        {
                            row.ReconDate = resultDate;
                        }
                        else
                        {
                            continue;
                        }

                        row.Account = "19990126507008";

                        row.Amount = Convert.ToDouble(reader.GetValue(7)?.ToString());

                        list.Add(row);
                    }
                }
            }

            if (list.Count > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                var outputFile = Path.Combine(outputFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_AirtelKE.txt");

                var lastRow = list.OrderByDescending(i => i.ReconDate).FirstOrDefault(c => c.ReconDate == list.Max(r => r.ReconDate));

                string toAppend = $"IMKE\t{lastRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(lastRow.ReconDate):MM/dd/yyyy}\t\t\t\t{-lastRow.Amount}\tKES\n";

                if (!string.IsNullOrEmpty(toAppend))
                {
                    File.WriteAllText(outputFile, toAppend);
                }
            }
        }

    }

    public class AirtelCols
    {
        public DateTime ReconDate { get; set; }
        public string Account { get; set; }
        public double Amount { get; set; }
    }
}
