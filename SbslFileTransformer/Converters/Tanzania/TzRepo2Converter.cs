using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class TzRepo2Converter
    {

        public TzRepo2Converter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public void ConvertFile(string inputFile, string outputFolder = "")
        {
            var content = File.ReadAllText(inputFile);
            var sDet = File.ReadAllLines(inputFile);
            string outputFile = "";
            string Account = "";
            double Amount = 0;
            var toAppend = "";
            DateTime baldate = DateTime.Now;
            string[] sGrp = content.Split("\n");
            if (content.Length != 0)
            {
                for (var i = 0; i < sGrp.Length - 1; i++)
                {
                    if (sGrp[i].Split('|')[1].Contains("Customer FX P&L for "))
                    {

                        baldate = DateTime.Parse(sGrp[i].Split('|')[1].Replace("Customer FX P&L for ", ""));
                    }
                }
                for (var i = 0; i < sGrp.Length - 1; i++)
                {
                    if (sGrp[i].Split('|')[2].Trim() != "")
                    {
                        if (sGrp[i].Split('|')[3] != "")
                        {
                            Account = GetAccountNumber(sGrp[i].Split('|')[2]);
                            Amount = Convert.ToDouble((sGrp[i].Split('|')[4].Trim()));
                            if (toAppend == "")
                            {
                                toAppend = $"IMKE\t{Account}\t\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(baldate):MM-dd-yyyy}\t\t\t\t{-1*Amount}\t{sGrp[i].Split('|')[2]}\n";
                            }
                            else
                            {
                                toAppend += $"IMKE\t{Account}\t\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(baldate):MM-dd-yyyy}\t\t\t\t{-1*Amount}\t{sGrp[i].Split('|')[2]}\n";
                            }

                        }
                    }

                }

                outputFile = Path.Combine(outputFolder, $"MultiCur_{baldate:yyyyMMdd}_{"Repo2_IMTZ"}.txt");

                WriteFile(outputFile, toAppend);
            }
        }

        public static void WriteFile(string path, string content)
        {
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (var sw = new StreamWriter(fs))
                    sw.Write(content);
            }
        }
        private string GetAccountNumber(string Currency)
        {
            if (Currency == "AUD")
                return "10010981001004";


            if (Currency == "CAD")
                return "10010881001010";

            if (Currency == "EUR")
                return "10000681001023";

            if (Currency == "INR")
                return "10001381001042";

            if (Currency == "JPY")
                return "10010781001045";

            if (Currency == "KES")
                return "10000181001047";

            if (Currency == "MUR")
                return "10031781001051";

            if (Currency == "GBP")
                return "10000581001037";

            if (Currency == "RWF")
                return "10000281001125";

            if (Currency == "ZAR")
                return "10011481001074";

            if (Currency == "CHF")
                return "10011181001014";

            if (Currency == "TZS")
                return "10000381001126";

            if (Currency == "USD")
                return "10000481001060";

            if (Currency == "AED")
                return "10011281001001";

            if (Currency == "UGX")
                return "10011681001127";

            if (Currency == "CNY")
                return "10012081001016";

            return "";
        }
    }
}
