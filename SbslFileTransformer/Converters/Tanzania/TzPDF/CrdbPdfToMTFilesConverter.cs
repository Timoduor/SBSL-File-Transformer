using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SbslFileTransformer.Converters
{
    public class CrdbPdfToMTFilesConverter
    {
        public void ConvertFile(string inputFile, string password = "", string outputFile = null)
        {
            var text = GetTextFromPdf(inputFile, password);

            var bankAcc = string.Empty;
            var currency = string.Empty;
            var transactions = new List<ExtractedTableCRDB>();
            var isNewTableLine = true;
            double closingBal = 0;

            ExtractedTableCRDB extractedTableLine = null;

            var areTableValues = false;

            foreach (var line in text.Split('\n', '\r'))
            {
                if (line.Contains("Posting Date")) areTableValues = true;

                //FOR CRDB
                if (line.ToLower().Contains("account:"))
                {
                    bankAcc = line.Split(":")[1].Trim();
                    continue;
                }

                if (line.ToLower().Contains("available balance"))
                {
                    currency = line.Split(' ')[3].Trim();
                    continue;
                }

                if (line.ToLower().Contains("cleared balance")) closingBal = Convert.ToDouble(line.Split(' ')[8]);

                if (areTableValues && Regex.IsMatch(line.Trim(), @"^\d{2}\.\d{2}\.\d{4}$") && isNewTableLine)
                {
                    extractedTableLine = new ExtractedTableCRDB();
                    transactions.Add(extractedTableLine); //-----------------------------------------===================
                    extractedTableLine.PostingDate = line.Trim();
                    continue;
                }

                if (areTableValues && Regex.IsMatch(line.Trim(), @"^\d{2}\:\d{2}\:\d{2}$") && isNewTableLine)
                {
                    extractedTableLine.PostingDate += " " + line.Trim();
                    isNewTableLine = false;
                    continue;
                }

                if (areTableValues && line.ToLower().Contains("ref:"))
                {
                    var split = line.Split(':');
                    extractedTableLine.Ref = split[1].Trim();

                    if (2 < split.Length) extractedTableLine.Ref += split[2].Trim();
                    continue;
                }

                if (areTableValues && !line.Contains("Posting Date") && !line.ToLower().Contains("ref:") &&
                    !Regex.IsMatch(line.Trim(), @"^\d{2}\.\d{2}\.\d{4}$")
                    && !Regex.IsMatch(line.Trim(), @"^\d{2}\:\d{2}\:\d{2}$") &&
                    !Regex.IsMatch(line.Trim(), @"\d{1,3}(,\d{3})*(\.\d+)?"))
                {
                    extractedTableLine.Details += line.Trim();
                    continue;
                }

                if (areTableValues && Regex.IsMatch(line.Trim(), @"^\d{2}\.\d{2}\.\d{4}$") && !isNewTableLine)
                {
                    extractedTableLine.ValueDate += line.Trim();
                    continue;
                }

                if (areTableValues && Regex.IsMatch(line.Trim(), @"^\d{2}\:\d{2}\:\d{2}$") && !isNewTableLine)
                {
                    extractedTableLine.ValueDate += " " + line.Trim();
                    isNewTableLine = true;
                    continue;
                }

                if (areTableValues && (Regex.IsMatch(line.Trim(), @"\d{1,3}(,\d{3})*\.\d{2}?$") ||
                                       Regex.IsMatch(line.Trim(), @"\d{1,3}(,\d{3})$")))
                {
                    var numbers = line.Trim().Split(' ');
                    extractedTableLine.Debit = numbers[0];
                    extractedTableLine.Credit = numbers[1];
                    extractedTableLine.BookBalance = numbers[2];

                    isNewTableLine = true;
                }
            }

            var lines = new StringBuilder();

            var balDate = DateTime.ParseExact(transactions.First().ValueDate, "dd.MM.yyyy HH:mm:ss",
                CultureInfo.InvariantCulture);

            lines.AppendLine(":20:" + "1");
            lines.AppendLine(":25:" + bankAcc);
            lines.AppendLine(":28C:" + "1/1");
            lines.AppendLine(":60M:" + $@"C{balDate:yyMMdd}{currency}0,00");

            foreach (var record in transactions)
            {
                var valDate =
                    DateTime.ParseExact(record.ValueDate, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                var valDateStr = valDate.ToString("yyMMdd");
                var valDateStr2 = valDate.ToString("MMdd");

                var dOrC = "C";

                var amountC = Convert.ToDouble(record.Credit);
                var amountD = Convert.ToDouble(record.Debit);

                var useC = true;
                if (amountC > 0)
                {
                    dOrC = "C";
                }
                else if (amountD > 0)
                {
                    useC = false;
                    dOrC = "D";
                }

                var narrative = $"{record.Ref?.Trim()}";
                var c61 =
                    $"{valDateStr}{valDateStr2}{dOrC}R{(useC ? amountC.ToString("N2").Replace(",", "").Replace(".", ",") : amountD.ToString("N2").Replace(",", "").Replace(".", ","))}S205{narrative}";

                lines.AppendLine($":61:{c61}  {record.Details?.Trim()}");
            }

            lines.AppendLine(":62F:" +
                             $@"C{balDate:yyMMdd}{currency}{closingBal.ToString("N2").Replace(",", "").Replace(".", ",")}");

            var fileName = Path.GetFileNameWithoutExtension(inputFile);

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyyMMdd}_{fileName.Substring(Math.Max(0, fileName.Length - 10))}.txt");
            }
            else
            {
                outputFile = Path.Combine(outputFile,
                    $"{DateTime.Now:yyyyMMdd}{new string(fileName.TakeLast(10).ToArray())}.txt");
            }

            File.WriteAllText(outputFile, lines.ToString());
        }


        public static string GetTextFromPdf(string path, string password = "")
        {
            var content = new StringBuilder();

            var readProps = new ReaderProperties().SetPassword(Encoding.Default.GetBytes(password));

            using (var reader = new PdfReader(path, readProps))
            {
                var pdfDocument = new PdfDocument(reader);

                var pages = pdfDocument.GetNumberOfPages();

                for (var i = 1; i <= pages; i++)
                {
                    var strategy = new SimpleTextExtractionStrategy();

                    var page = pdfDocument.GetPage(i);

                    var text = PdfTextExtractor.GetTextFromPage(page, strategy);

                    content.Append(text);
                }
            }

            return content.ToString();
        }

        public class ExtractedTableCRDB
        {
            public string PostingDate { get; set; }
            public string Details { get; set; }
            public string Ref { get; set; }
            public string ValueDate { get; set; }
            public string Debit { get; set; }
            public string Credit { get; set; }
            public string BookBalance { get; set; }
        }
    }
}