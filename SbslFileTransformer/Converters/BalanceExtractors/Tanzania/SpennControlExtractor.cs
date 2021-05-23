using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.BalanceExtractors.Tanzania
{
    public class SpennControlExtractor
    {
        public SpennControlExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFolder)
        {
            var bal = new W2BCols();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream, new ExcelReaderConfiguration { AutodetectSeparators = new[] { ',', '\t' } }))
                {
                    while (reader.Read())
                    {
                        if (DateTime.TryParseExact(reader.GetValue(3)?.ToString(), "yyyyMMd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                        {
                            bal.Date = result;
                        }

                        bal.Amount = Convert.ToDouble(reader.GetValue(2)?.ToString()) * -1;

                        bal.Account = "30990326501022";
                    }
                }
            }

            GenerateMultiCurr(bal, inputFile, outputFolder);
        }

        private void GenerateMultiCurr(W2BCols item, string inputFile, string outputFolder)
        {
            if (item != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                var outputFile = Path.Combine(outputFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_SPEN_CTRL_TZ.txt");

                string toAppend = $"IMTZ\t{item.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(item.Date):MM/dd/yyyy}\t\t\t\t{item.Amount.ToString("N2")}\tTZS\n";

                if (!string.IsNullOrEmpty(toAppend))
                {
                    File.WriteAllText(outputFile, toAppend);
                }
            }
        }
    }
}
