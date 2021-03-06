using CsvHelper;
using ExcelDataReader;
using Microsoft.Extensions.DependencyInjection;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class CDMBalanceExtractor
    {

        public string Entity { get; set; }
        public IServiceScopeFactory ServiceScopeFactory { get; set; }

        public CDMBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public async Task ConvertFile(string inputFile, string outputFolder)
        {
            //Replace empties with zeros in columns 5 and 6

            var list = new List<CdmCols>();

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

                        var value = reader.GetValue(0)?.ToString();

                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        var row = new CdmCols();

                        DateTime resultDate;

                        if (DateTime.TryParseExact(reader.GetValue(1)?.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out resultDate))
                        {
                            row.ReconDate = resultDate;
                        }
                        else
                        {
                            continue;
                        }

                        row.Account = reader.GetValue(4)?.ToString();

                        row.AmountGL = Convert.ToDouble(reader.GetValue(7)?.ToString());

                        row.AmountMC = Convert.ToDouble(reader.GetValue(6)?.ToString());

                        list.Add(row);
                    }
                }
            }

            if (list.Count > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                var outputFile = Path.Combine(outputFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_cdmKE.txt");
                var outputFileGL = Path.Combine(outputFolder, $"GLAccounts_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_cdmKE.txt");

                Dictionary<string, string> lookUp = new Dictionary<string, string>();

                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var pairs = dbContext.Accounts.Select(a => new { a.Number, a.Name });

                    foreach (var acc in pairs)
                    {
                        lookUp.TryAdd(acc.Number, acc.Name);
                    }
                }

                var toAppend = new StringBuilder();
                var toAppendGL = new StringBuilder();

                foreach (var row in list)
                {
                    var success = long.TryParse(row.Account, out long result);

                    var account = success ? result.ToString() : row.Account;

                    toAppend.Append($"{Entity}\t{account}\tCDM\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(row.ReconDate):MM/dd/yyyy}\t\t\t\t{row.AmountMC}\t{GetAccountCurrency(row.Account)}\n");

                    toAppendGL.Append($"{Entity}\t{account}\tCDM\t\t\t\t\t\t\t\t{GetAccountName(row.Account, lookUp)}\tCDM\tA\tAsset\tTRUE\tTRUE\t\t{GetAccountCurrency(row.Account)}\t{ContentHelpers.GetLastDayOfTheMonth(row.ReconDate):MM/dd/yyyy}\t\t\t{row.AmountGL}\n");
                }

                //write multicurr file
                var text = toAppend.ToString();

                if (!string.IsNullOrEmpty(text))
                {
                    await File.WriteAllTextAsync(outputFile, text);
                }

                //write gl_acc file
                var text2 = toAppendGL.ToString();

                if (!string.IsNullOrEmpty(text2))
                {
                    await File.WriteAllTextAsync(outputFileGL, text2);
                }

            }
        }

        private string GetAccountCurrency(string account)
        {
            string currency = "KES";

            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var curr = dbContext.Accounts.FirstOrDefault(a => a.Number == account).Currency;

                currency = string.IsNullOrEmpty(curr) ? currency : curr;
            }

            return currency;

        }

        private string GetAccountName(string accountNumber, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(accountNumber))
            {
                return dict[accountNumber];
            }
            else
            {
                return accountNumber;
            }
        }
    }

    public class CdmCols
    {
        public DateTime ReconDate { get; set; }
        public string Account { get; set; }
        public double AmountMC { get; set; }
        public double AmountGL { get; set; }
    }
}
