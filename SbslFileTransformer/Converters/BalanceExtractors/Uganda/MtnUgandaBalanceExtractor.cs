using ExcelDataReader;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System;
using System.Linq;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.BalanceExtractors.Uganda
{
    public class MtnUgandaBalanceExtractor
    {
        public MtnUgandaBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public void ConvertFile(string inputFile, string outputFolder)
        {
            List<MtnCols> list = new List<MtnCols>();
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
                        if (reader.GetValue(0)?.ToString().ToLower().Contains("DATE") ?? false)
                            continue;

                        MtnCols row = new MtnCols();

                        if (DateTime.TryParseExact(reader.GetValue(0)?.ToString(), "yyyy-MM-dd HH:mm:ss",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                            row.ReconDate = resultDate;
                        else
                            continue;

                        row.Account = "220003015";
                        row.Amount = Convert.ToDouble(reader.GetValue(22)?.ToString());
                        list.Add(row);
                    }
                }
            }

            if (list.Count > 0)
            {
                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                string outputFile = Path.Combine(outputFolder,
                    $"MultiCurr_{DateTime.Now:dd_MM_yyyy}_{fileNameToAppend}_MtnUG.txt");

                MtnCols firstRow = list.OrderByDescending(i => i.ReconDate)
                    .FirstOrDefault(c => c.ReconDate == list.Max(r => r.ReconDate));

                string toAppend =
                    $"IMUG\t{firstRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(firstRow.ReconDate):MM/dd/yyyy}\t\t\t\t{firstRow.Amount}\tUGX\n";

                if (!string.IsNullOrEmpty(toAppend)) 
                    File.WriteAllText(outputFile, toAppend);
            }
        }

        public class MtnCols
        {
            public DateTime ReconDate { get; set; }
            public string Account { get; set; }
            public double Amount { get; set; }
        }
    }
}
