using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.Extensions.DependencyInjection;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.BalanceExtractors.Kenya
{
    public class ImsBalanceExtractor
    {
        public ImsBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public string Entity { get; set; }
        public IServiceScopeFactory ServiceScopeFactory { get; set; }

        public async Task ConvertFile(string inputFile, string outputFolder, string entity = "IMKE")
        {
            //Replace empties with zeros in columns 5 and 6

            List<CdmCols> list = new List<CdmCols>();

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
                        string value = reader.GetValue(0)?.ToString();

                        if (string.IsNullOrEmpty(value)) continue;

                        CdmCols row = new CdmCols();


                        if (DateTime.TryParseExact(reader.GetValue(0)?.ToString(), "MM/dd/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                        {
                            row.ReconDate = resultDate;
                        }
                        else if (DateTime.TryParse(reader.GetValue(0)?.ToString(), out resultDate))
                        {
                            row.ReconDate = resultDate;
                        }
                        else if (int.TryParse(reader.GetValue(0)?.ToString(), out int intRes))
                        {
                            if (intRes.FromExcelSerialDate(out resultDate)) row.ReconDate = resultDate;
                        }
                        else
                        {
                            continue;
                        }

                        row.Account = reader.GetValue(2)?.ToString();

                        row.AmountMC = Convert.ToDouble(reader.GetValue(3)?.ToString());

                        list.Add(row);
                    }
                }
            }

            if (list.Count > 0)
            {
                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                string outputFile = Path.Combine(outputFolder,
                    $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_IMS_{entity}.txt");

                Dictionary<string, string> lookUp = new Dictionary<string, string>();

                using (IServiceScope scope = ServiceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var pairs = dbContext.Accounts.Select(a => new { a.Number, a.Name });

                    foreach (var acc in pairs) lookUp.TryAdd(acc.Number, acc.Name);
                }

                StringBuilder toAppend = new StringBuilder();

                foreach (CdmCols row in list)
                {
                    //var success = long.TryParse(row.Account, out var result);

                    string account = row.Account;

                    toAppend.Append(
                        $"{Entity}\t{account}\tIMS\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(row.ReconDate):MM/dd/yyyy}\t\t\t\t{row.AmountMC}\tKES\n");
                }

                //write multicurr file
                string text = toAppend.ToString();

                if (!string.IsNullOrEmpty(text))
                    await File.WriteAllTextAsync(outputFile, text);
            }
        }

        private string GetAccountCurrency(string account)
        {
            string currency = "KES";

            using (IServiceScope scope = ServiceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                string curr = dbContext.Accounts.FirstOrDefault(a => a.Number == account).Currency;

                currency = string.IsNullOrEmpty(curr) ? currency : curr;
            }

            return currency;
        }

        private string GetAccountName(string accountNumber, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(accountNumber))
                return dict[accountNumber];
            return accountNumber;
        }
    }

}
