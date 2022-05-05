using ExcelDataReader;
using Microsoft.Extensions.DependencyInjection;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.VisionFinacleMatcher;

namespace SbslFileTransformer.Converters.BalanceExtractors.Kenya
{
    public class VisionMulticurrBalanceExtractor
    {
        public string Entity { get; set; }
        public IServiceScopeFactory ServiceScopeFactory { get; set; }

        public VisionMulticurrBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFolder, VisionRecordType visionRecordType)
        {
            //Replace empties with zeros in columns 5 and 6

            List<VisionBalance> list = new List<VisionBalance>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;

                if (Path.GetExtension(inputFile).ToLower().Contains("csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {
                    int count = 0;

                    while (reader.Read())
                    {
                        count++;

                        if (count <= 11)
                            continue;

                        VisionBalance row = new VisionBalance();

                        if (DateTime.TryParseExact(reader.GetValue(0)?.ToString(), "yyyy-MM-dd HH:mm:ss.s",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                            row.BankingDate = resultDate;
                        else
                            continue;

                        if (DateTime.TryParseExact(reader.GetValue(3)?.ToString(), "yyyy-MM-dd HH:mm:ss.s",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate2))
                            row.ExpiryDate = resultDate2;

                        row.AvailableBalance = Convert.ToDouble(string.IsNullOrEmpty(reader.GetValue(8)?.ToString().Trim()) ? "0" : reader.GetValue(8).ToString());
                        row.Balance = Convert.ToDouble(string.IsNullOrEmpty(reader.GetValue(6)?.ToString().Trim()) ? "0" : reader.GetValue(6).ToString());
                        row.CardLimit = Convert.ToDouble(string.IsNullOrEmpty(reader.GetValue(9)?.ToString().Trim()) ? "0" : reader.GetValue(9).ToString());
                        row.CardNo = reader.GetValue(2)?.ToString();
                        row.ClientShortName = reader.GetValue(4)?.ToString();
                        row.ContractNumber = reader.GetValue(1)?.ToString();
                        row.Currency = Convert.ToInt32(string.IsNullOrEmpty(reader.GetValue(5)?.ToString().Trim()) ? "0" : reader.GetValue(5).ToString());
                        row.DelCount = Convert.ToInt32(string.IsNullOrEmpty(reader.GetValue(11)?.ToString().Trim()) ? "0" : reader.GetValue(11).ToString());
                        row.FxRate = Convert.ToDouble(string.IsNullOrEmpty(reader.GetValue(12)?.ToString().Trim()) ? "0" : reader.GetValue(12).ToString());
                        row.Product = reader.GetValue(10)?.ToString();
                        row.TotalBalance = Convert.ToDouble(string.IsNullOrEmpty(reader.GetValue(7)?.ToString().Trim()) ? "0" : reader.GetValue(7).ToString());

                        row.Account = this.GetAccountNumber(row.Product);

                        list.Add(row);
                    }
                }
                stream.Close();
            }

            IEnumerable<VisionBalance> distinct = list.GroupBy(b => b.ContractNumber).Select(x => x.FirstOrDefault());

            IEnumerable<MultiCurrVision> summed = distinct.GroupBy(r => r.Account).Select(b => new MultiCurrVision
            {
                Account = b.FirstOrDefault().Account,
                Amount = b.Sum(x => x.TotalBalance),
                BalanceDate = b.FirstOrDefault().BankingDate
            });

            if (list.Count > 0)
            {
                CreateMultiCurrFile(inputFile, outputFolder, summed, visionRecordType);
            }
        }

        private static void CreateMultiCurrFile(string inputFile, string outputFolder, IEnumerable<MultiCurrVision> balances, VisionRecordType visionRecordType)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            string outputFile = Path.Combine(outputFolder,
                $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_{visionRecordType}_VisKE.txt");

            StringBuilder bals = new StringBuilder();

            foreach (var row in balances)
            {
                string toAppend =
                    $"IMKE\t{row.Account}\tCARDS KENYA\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(row.BalanceDate):MM/dd/yyyy}\t\t\t\t{-Math.Round(row.Amount, 2)}\tKES\n";

                bals.Append(toAppend);
            }

            if (!string.IsNullOrEmpty(bals.ToString())) File.WriteAllText(outputFile, bals.ToString());
        }

        private string GetAccountNumber(string inputValue)
        {
            string account = "";


            if (inputValue.ToUpper().Contains("CLASSIC"))
            {
                account = "18000113002017";
            }
            if (inputValue.ToUpper().Contains("GOLD"))
            {
                account = "18000113001018";
            }
            if (inputValue.ToUpper().Contains("INFINITE"))
            {
                account = "18000113003016";
            }
            if (inputValue.ToUpper().Contains("TAMARIND"))
            {
                account = "18000113004015";
            }

            return account;
        }

        private class MultiCurrVision
        {
            public DateTime BalanceDate { get; set; }
            public string Account { get; set; }
            public double Amount { get; set; }

        }
    }
}
