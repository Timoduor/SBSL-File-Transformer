using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs.Converters;

namespace SbslFileTransformer.Converters.BalanceExtractors.Kenya
{
    public class ImsBalanceExtractor
    {
        public ImsBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFolder)
        {
            var list = new List<AirtelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {

                IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {

                    while (reader.Read())
                    {
                        if (reader.GetValue(0)?.ToString().ToLower().Contains("transaction") ?? false) continue;

                        var row = new AirtelCols();

                        if (DateTime.TryParseExact(reader.GetValue(2)?.ToString(), "dd/MM/yyyy HH:mm",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var resultDate))
                            row.ReconDate = resultDate;
                        else
                            continue;

                        row.Account = "19990126507008";//TODO: NEEDS TO BE CHANGED

                        row.Amount = Convert.ToDouble(reader.GetValue(7)?.ToString());

                        list.Add(row);
                    }
                }
            }

            if (list.Count > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                var outputFile = Path.Combine(outputFolder,
                    $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_IMS_KE.txt");

                var lastRow = list.OrderByDescending(i => i.ReconDate)
                    .FirstOrDefault(c => c.ReconDate == list.Max(r => r.ReconDate));

                var toAppend =
                    $"IMKE\t{lastRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(lastRow.ReconDate):MM/dd/yyyy}\t\t\t\t{-lastRow.Amount}\tKES\n";//TODO: NEEDS SOME CHANGES

                if (!string.IsNullOrEmpty(toAppend)) File.WriteAllText(outputFile, toAppend);
            }
        }
    }

}
