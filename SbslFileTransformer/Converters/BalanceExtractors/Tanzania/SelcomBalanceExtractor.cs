using CsvHelper;
using HtmlAgilityPack;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SbslFileTransformer.Converters
{
    public class SelcomBalanceExtractor
    {
        public SelcomBalanceExtractor()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFolder)
        {
            var list = new List<B2WSelcomBalCols>();

            var ext = Path.GetExtension(inputFile).ToLower();


            switch (GetMBType(inputFile))
            {
                case MBTypeTz.SELCOM:
                    GetHtmlSelcomData(inputFile, list);
                    break;

                case MBTypeTz.B2W:
                    GetHtmlSelcomData(inputFile, list);
                    break;

                case MBTypeTz.W2B:
                    var list2 = new List<W2BCols>();

                    GetHtmlW2BData(inputFile, list2);

                    //GenerateMultiCurr2(list2, inputFile, outputFolder);

                    break;
            }



            if (list.Count > 0 && GetMBType(inputFile) != MBTypeTz.W2B)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                var toAppend2 = GetMBType(inputFile) == MBTypeTz.B2W ? "B2W" : "SPEN";

                var outputFile = Path.Combine(outputFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_MB_{toAppend2}_TZ.txt");

                var lastRow = list.LastOrDefault(c => c.Date == list.Max(r => r.Date) && (c.TransType.ToUpper() == "DEBIT" || c.TransType.ToUpper() == "CREDIT" || c.TransType.ToUpper() == "CHARGE"));

                if (lastRow != null)
                {
                    string toAppend = $"IMTZ\t{lastRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(lastRow.Date):MM/dd/yyyy}\t\t\t\t{lastRow.CBal}\tTZS\n";

                    if (!string.IsNullOrEmpty(toAppend))
                    {
                        File.WriteAllText(outputFile, toAppend);
                    }
                }
            }


        }

        private void GenerateMultiCurr2(List<W2BCols> list, string inputFile, string outputFolder)
        {
            if (list.Count > 0)
            {
                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

                var outputFile = Path.Combine(outputFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_MB_W2B_TZ.txt");

                var lastRow = list.LastOrDefault(c => c.Date == list.Max(r => r.Date));

                if (lastRow != null)
                {
                    string toAppend = $"IMTZ\t{lastRow.Account}\tMobile banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(lastRow.Date):MM/dd/yyyy}\t\t\t\t{lastRow.Amount}\tTZS\n";

                    if (!string.IsNullOrEmpty(toAppend))
                    {
                        File.WriteAllText(outputFile, toAppend);
                    }
                }
            }

        }

        private void GetHtmlW2BData(string inputFile, List<W2BCols> list)
        {
            var content = File.ReadAllText(inputFile);

            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(content);

            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
            {
                int count = 0;

                foreach (HtmlNode row in table.SelectNodes("tr"))
                {
                    if (count <= 0)
                    {
                        count++;
                        continue;
                    }

                    var selcomRow = new W2BCols();

                    var dateString = row.SelectNodes("td")[0].InnerText;

                    if (DateTime.TryParseExact(dateString, "M/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                    {
                        selcomRow.Date = resultDate;
                    }
                    else if (DateTime.TryParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    {
                        selcomRow.Date = result;
                    }
                    else
                    {
                        continue;
                    }

                    string amountString = row.SelectNodes("td")[7].InnerText;

                    string amount = string.IsNullOrEmpty(amountString) ? "0" : amountString;

                    selcomRow.Amount = Convert.ToDouble(amount);

                    selcomRow.Processed = row.SelectNodes("td")[1].InnerText;

                    selcomRow.TransID = row.SelectNodes("td")[2].InnerText;

                    selcomRow.Reference = row.SelectNodes("td")[3].InnerText;

                    selcomRow.Terminal = row.SelectNodes("td")[4].InnerText;

                    selcomRow.Account2 = row.SelectNodes("td")[5].InnerText;

                    selcomRow.Result = row.SelectNodes("td")[6].InnerText;

                    selcomRow.Channel = row.SelectNodes("td")[8].InnerText;

                    selcomRow.Account = GetAccountNumber(inputFile);

                    list.Add(selcomRow);
                }
            }

            ConvertToCsvFile(list, inputFile);
        }


        private void GetHtmlSelcomData(string inputFile, List<B2WSelcomBalCols> list)
        {
            var content = File.ReadAllText(inputFile);

            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(content);

            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
            {
                int count = 0;

                foreach (HtmlNode row in table.SelectNodes("tr"))
                {
                    if (count <= 0)
                    {
                        count++;
                        continue;
                    }

                    var selcomRow = new B2WSelcomBalCols();

                    var dateString = row.SelectNodes("td")[0].InnerText;

                    if (DateTime.TryParseExact(dateString, "M/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resultDate))
                    {
                        selcomRow.Date = resultDate;
                    }
                    else if (DateTime.TryParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    {
                        selcomRow.Date = result;
                    }
                    else
                    {
                        continue;
                    }

                    string amountString = row.SelectNodes("td")[9].InnerText;

                    string amount = string.IsNullOrEmpty(amountString) ? "0" : amountString;

                    selcomRow.CBal = Convert.ToDouble(amount);

                    selcomRow.Terminal = row.SelectNodes("td")[1].InnerText;

                    selcomRow.TransType = row.SelectNodes("td")[2].InnerText;

                    string amountString2 = row.SelectNodes("td")[3].InnerText;

                    string amount2 = string.IsNullOrEmpty(amountString2) ? "0" : amountString2;

                    selcomRow.Amount = Convert.ToDouble(amount2);

                    selcomRow.UtilityType = row.SelectNodes("td")[4].InnerText;

                    selcomRow.UtilityReference = row.SelectNodes("td")[5].InnerText;

                    selcomRow.Reference = row.SelectNodes("td")[6].InnerText;

                    selcomRow.TransID = row.SelectNodes("td")[7].InnerText;

                    string amountString3 = row.SelectNodes("td")[8].InnerText;

                    string amount3 = string.IsNullOrEmpty(amountString3) ? "0" : amountString3;

                    selcomRow.OBal = Convert.ToDouble(amount3);

                    selcomRow.Account = GetAccountNumber(inputFile);

                    list.Add(selcomRow);
                }
            }

            ConvertToCsvFile(list, inputFile);

        }

        private void ConvertToCsvFile<T>(List<T> rows, string inputFile, string outputFile = null)
        {
            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }


            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<T>();
                    csv.NextRecord();

                    foreach (var row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }

        public MBTypeTz GetMBType(string inputFile)
        {
            if (inputFile.ToLower().Contains("w2b") && inputFile.ToLower().Contains("portal"))
                return MBTypeTz.W2B;
            if (inputFile.ToLower().Contains("b2w") && inputFile.ToLower().Contains("portal"))
                return MBTypeTz.B2W;
            if (inputFile.ToLower().Contains("spenn") && inputFile.ToLower().Contains("selcom"))
                return MBTypeTz.SELCOM;

            return MBTypeTz.SELCOM;
        }

        private string GetAccountNumber(string inputFile)
        {
            string ret;

            switch (GetMBType(inputFile))
            {
                case MBTypeTz.W2B:
                    ret = "30010326501012";
                    break;
                case MBTypeTz.B2W:
                    ret = "30990326501010";
                    break;
                case MBTypeTz.SELCOM:
                    ret = "30990326501023";
                    break;
                default:
                    ret = "";
                    break;
            }

            return ret;
        }

        private class B2WSelcomBalCols
        {
            public DateTime Date { get; set; }
            public string Terminal { get; set; }
            public string TransType { get; set; }
            public double Amount { get; set; }
            public string UtilityType { get; set; }
            public string UtilityReference { get; set; }
            public string Reference { get; set; }
            public string TransID { get; set; }
            public double OBal { get; set; }
            public double CBal { get; set; }
            public string Account { get; set; }

        }
    }

    public enum MBTypeTz
    {
        B2W,
        SELCOM,
        W2B
    }




    public class W2BCols
    {
        public DateTime Date { get; set; }
        public string Processed { get; set; }
        public string TransID { get; set; }
        public string Reference { get; set; }
        public string Terminal { get; set; }
        public string Account2 { get; set; }
        public string Result { get; set; }
        public double Amount { get; set; }
        public string Channel { get; set; }
        public string Account { get; set; }
    }
}
