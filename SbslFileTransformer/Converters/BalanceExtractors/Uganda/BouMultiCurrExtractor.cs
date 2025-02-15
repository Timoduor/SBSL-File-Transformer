using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using ExcelDataReader;

using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.BalanceExtractors.Uganda
{
    public class BouMultiCurrExtractor
    {
        public static Dictionary<string, string> BouMultiCurrAccount = new Dictionary<string, string>()
        {
            { "EUR", "59990610505001" },
            { "GBP", "59990510505002" },
            { "KES", "59990110505003" },
            { "UGX", "59991610501001" },
            { "USD", "59990410505004" },
        };

        public BouMultiCurrExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFolder)
        {
            var multiCurrCols = new List<BouCols>();

            var folderName = new DirectoryInfo(inputFile).Parent.Name;

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
                        var row = new BouCols();

                        var date = reader.GetValue(0);

                        if (DateTime.TryParseExact(date?.ToString(), "dd.MM.yyyy HH:mm:ss",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out var resultDate))
                        {
                            row.ProcessingDateAndTime = resultDate;
                        }
                        else
                        {
                            continue;
                        }

                        var balance = reader.GetValue(9);

                        if (string.IsNullOrEmpty(balance?.ToString()))
                        {
                            continue;
                        }

                        if (decimal.TryParse(balance?.ToString(), out var result))
                        {
                            row.Balance = result;
                        }

                        multiCurrCols.Add(row);
                    }
                }
            }

            if (multiCurrCols.Count > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 21)).Replace(" ", "");

                var outputFile = Path.Combine(outputFolder,
                    $"MultiCurr_{DateTime.Now:dd_MM_yyyy}_{fileNameToAppend}_BOUG.txt");

                var lastRow = multiCurrCols.OrderBy(i => i.ProcessingDateAndTime)
                    .LastOrDefault(c => c.ProcessingDateAndTime == multiCurrCols.Max(r => r.ProcessingDateAndTime));

                var currency = new DirectoryInfo(inputFile).Parent.Name;

                var toAppend =
                    $"IMUG\t{BouMultiCurrAccount[currency]}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(lastRow.ProcessingDateAndTime):MM/dd/yyyy}\t\t\t\t{lastRow.Balance}\t{currency}\n";

                if (!string.IsNullOrEmpty(toAppend))
                {
                    File.WriteAllText(outputFile, toAppend);
                }
            }
        }
    }

    public class BouCols
    {
        public DateTime ProcessingDateAndTime { get; set; }

        public decimal Balance { get; set; }

        public string FolderName { get; set; }
    }
}
