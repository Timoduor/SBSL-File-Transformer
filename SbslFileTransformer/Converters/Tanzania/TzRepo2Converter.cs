using System;
using System.IO;
using System.Text;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.Tanzania
{
    public class TzRepo2Converter
    {

        public TzRepo2Converter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public void ConvertFile(string inputFile, string outputFolder = "")
        {
            string content = File.ReadAllText(inputFile);
            string[] sDet = File.ReadAllLines(inputFile);
            string outputFile = "";
            string Account = "";
            double Amount = 0;
            string toAppend = "";
            DateTime baldate = DateTime.Now;
            string[] sGrp = content.Split("\n");
            if (content.Length != 0)
            {
                for (int i = 0; i < sGrp.Length - 1; i++)
                {
                    if (sGrp[i].Split('|')[1].Contains("Customer FX P&L for "))
                    {

                        baldate = DateTime.Parse(sGrp[i].Split('|')[1].Replace("Customer FX P&L for ", ""));
                    }
                }
                for (int i = 0; i < sGrp.Length - 1; i++)
                {
                    if (sGrp[i].Split('|')[2].Trim() != "")
                    {
                        if (sGrp[i].Split('|')[3] != "")
                        {
                            Account = this.GetAccountNumber(sGrp[i].Split('|')[2]);
                            Amount = Convert.ToDouble((sGrp[i].Split('|')[4].Trim()));
                            if (toAppend == "")
                            {
                                toAppend = $"IMTZ\t{Account}\t\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(baldate):MM-dd-yyyy}\t\t\t\t{-1 * Amount}\t{sGrp[i].Split('|')[2]}\n";
                            }
                            else
                            {
                                toAppend += $"IMTZ\t{Account}\t\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(baldate):MM-dd-yyyy}\t\t\t\t{-1 * Amount}\t{sGrp[i].Split('|')[2]}\n";
                            }

                        }
                    }

                }

                outputFile = Path.Combine(outputFolder, $"MultiCurr_{baldate:yyyyMMdd}_{"Repo2_IMTZ"}.txt");

                WriteFile(outputFile, toAppend);
            }
        }

        public static void WriteFile(string path, string content)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                    sw.Write(content);
            }
        }
        private string GetAccountNumber(string Currency)
        {
            if (Currency == "AUD")
                // return "10010981001004";
                return "10010981001004";


            if (Currency == "CAD")
                // return "10010881001010";
                return "30010881001015";

            if (Currency == "EUR")
                return "30000681001013";

            if (Currency == "INR")
                return "30011381001013";

            if (Currency == "JPY")
                return "30030781001015";

            if (Currency == "KES")
                return "30010181001013";

            if (Currency == "MUR")
                return "10031781001051";

            if (Currency == "GBP")
                return "30000581001013";

            if (Currency == "RWF")
                return "30010281001013";

            if (Currency == "ZAR")
                return "30011481001013";

            if (Currency == "CHF")
                return "30981181001013";

            if (Currency == "TZS")
                return "30000381001006";

            if (Currency == "USD")
                return "30000481001013";

            if (Currency == "AED")
                return "10011281001001";

            if (Currency == "UGX")
                return "30061681001013";

            if (Currency == "CNY")
                return "10012081001016";

            return "";
        }
    }
}
