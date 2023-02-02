using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System;
using System.Linq;

namespace SbslFileTransformer.Converters.BalanceExtractors.Uganda
{
    public class AirtelUgandaBalanceExtractor
    {
        public AirtelUgandaBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public void ConvertFile(string inputFile, string outputFolder)
        {
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
                        if (reader.GetValue(1)?.ToString().ToLower().Contains("SERVICE_NAME") ?? false)
                            continue;

                        AirtelCols row = new AirtelCols();

                        if (DateTime.TryParseExact(reader.GetValue(0)?.ToString(), "dd/MM/yyyy HH:mm",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                            row.ReconDate = resultDate;
                        else if (DateTime.TryParseExact(reader.GetValue(0)?.ToString(), "dd/MM/yyyy HH:mm:ss",
                                     CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate2))
                            row.ReconDate = resultDate2;
                        else
                            continue;

                        row.Account = "220003016";

                        row.Amount = Convert.ToDouble(reader.GetValue(8)?.ToString());

                        list.Add(row);
                    }
                }
            }



            if (list.Count > 0)
            {
                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 21)).Replace(" ", "");

                string outputFile = Path.Combine(outputFolder,
                    $"MultiCurr_{DateTime.Now:dd_MM_yyyy}_{fileNameToAppend}_AirtelUG.txt");

                AirtelCols firstRow = list.OrderByDescending(i => i.ReconDate)
                    .FirstOrDefault(c => c.ReconDate == list.Max(r => r.ReconDate));

                string toAppend =
                    $"IMUG\t{firstRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(firstRow.ReconDate):MM/dd/yyyy}\t\t\t\t{firstRow.Amount}\tUGX\n";

                if (!string.IsNullOrEmpty(toAppend))
                    File.WriteAllText(outputFile, toAppend);
            }
        }
        public void ConvertFile_B2W(string inputFile, string outputFolder)
        {
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
                        if (reader.GetValue(1)?.ToString().ToLower().Contains("SERVICE_NAME") ?? false)
                            continue;

                        AirtelCols row = new AirtelCols();

                        if (DateTime.TryParseExact(reader.GetValue(0)?.ToString(), "dd/MM/yyyy HH:mm",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                            row.ReconDate = resultDate;
                        else if (DateTime.TryParseExact(reader.GetValue(0)?.ToString(), "dd/MM/yyyy HH:mm:ss",
                                     CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate2))
                            row.ReconDate = resultDate2;
                        else
                            continue;

                        row.Account = "115001205";

                        row.Amount = Convert.ToDouble(reader.GetValue(9)?.ToString());

                        list.Add(row);
                    }
                }
            }



            if (list.Count > 0)
            {
                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 21)).Replace(" ", "");

                string outputFile = Path.Combine(outputFolder,
                    $"MultiCurr_{DateTime.Now:dd_MM_yyyy}_{fileNameToAppend}_AirtelUG.txt");

                AirtelCols firstRow = list.OrderByDescending(i => i.ReconDate)
                    .FirstOrDefault(c => c.ReconDate == list.Max(r => r.ReconDate));

                string toAppend =
                    $"IMUG\t{firstRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(firstRow.ReconDate):MM/dd/yyyy}\t\t\t\t{firstRow.Amount}\tUGX\n";

                if (!string.IsNullOrEmpty(toAppend)) 
                    File.WriteAllText(outputFile, toAppend);
            }
        }

        public class AirtelCols
        {
            public DateTime ReconDate { get; set; }
            public string Account { get; set; }
            public double Amount { get; set; }
        }
    }
}
