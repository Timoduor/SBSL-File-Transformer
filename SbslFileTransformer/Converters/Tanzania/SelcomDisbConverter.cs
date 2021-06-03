using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.Tanzania
{
    public class SelcomDisbConverter
    {
        public SelcomDisbConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = "")
        {
            List<B2WSelcomBalCols> list = new List<B2WSelcomBalCols>();

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

                    list.Add(selcomRow);
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");

                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_SELC_DISB_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            ConvertToCsvFile(list, inputFile, outputFile);
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

        }
    }

}
