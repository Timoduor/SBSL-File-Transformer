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
    public class DtbPdfToMTFilesConverter
    {
        public void ConvertFile(string inputFile, string password = "001402498", string outputFile = null)
        {
            string text = GetTextFromPdf(inputFile, password);

            string bankAcc = string.Empty;
            string currency = string.Empty;
            List<ExtractedTableCRDB> transactions = new List<ExtractedTableCRDB>();

            bool needsBookBalance = false;

            ExtractedTableCRDB extractedTableLine = new ExtractedTableCRDB();

            foreach (string line in text.Split('\n', '\r'))
            {
                if (Regex.IsMatch(line.Trim(), @"\d{2}-[A-Z]{1}[a-z]{2}-\d{4} \d{2}-[A-Z]{1}[a-z]{2}-\d{4}") &&
                    !line.Contains("Opening Balance") || needsBookBalance)
                {
                    string[] parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    int len = parts.Length;

                    if (len < 5 && !needsBookBalance)
                    {
                        needsBookBalance = true;

                        extractedTableLine = new ExtractedTableCRDB();

                        extractedTableLine.PostingDate =
                            DateTime.ParseExact(parts[0], "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        extractedTableLine.ValueDate =
                            DateTime.ParseExact(parts[1], "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        extractedTableLine.Details = parts[2] + parts[3];

                        continue;
                    }

                    if (!needsBookBalance)
                    {
                        extractedTableLine = new ExtractedTableCRDB();

                        extractedTableLine.PostingDate =
                            DateTime.ParseExact(parts[0], "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        extractedTableLine.ValueDate =
                            DateTime.ParseExact(parts[1], "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                        extractedTableLine.Details = parts[2] + parts[3];
                        extractedTableLine.Debit = parts[len - 3];
                        extractedTableLine.Credit = parts[len - 2];
                        extractedTableLine.BookBalance = parts[len - 1];
                        extractedTableLine.Ref = parts[len - 4] + parts[len - 5];


                        transactions.Add(extractedTableLine);

                        continue;
                    }

                    if (Regex.IsMatch(line.Trim(), @"\d{1,2}[\,\.]{1}\d{1,2}") && needsBookBalance)
                    {
                        extractedTableLine.Debit = parts[2];
                        extractedTableLine.Credit = parts[3];
                        extractedTableLine.BookBalance = parts[4];
                        extractedTableLine.Ref = parts[0] + parts[1];

                        needsBookBalance = false;

                        transactions.Add(extractedTableLine);
                    }


                    continue;
                }

                if (line.Contains("Account No.:"))
                {
                    bankAcc = line.Split(':')[1].Trim();
                    continue;
                }

                if (line.Contains("Currency:")) currency = line.Split(':')[1].Trim();
            }

            if (transactions.Count == 0) throw new Exception($"No transactions found in DTB PDF file {inputFile}");

            double closingBal = Convert.ToDouble(transactions.Last().BookBalance);

            StringBuilder lines = new StringBuilder();

            DateTime balDate = transactions.First().ValueDate;

            lines.AppendLine(":20:" + "1");
            lines.AppendLine(":25:" + bankAcc);
            lines.AppendLine(":28C:" + "1/1");
            lines.AppendLine(":60M:" + $@"C{balDate:yyMMdd}{currency}0,00");

            foreach (ExtractedTableCRDB record in transactions)
            {
                DateTime valDate = record.ValueDate;
                string valDateStr = valDate.ToString("yyMMdd");
                string valDateStr2 = valDate.ToString("MMdd");

                string dOrC = "C";

                double amountC = Convert.ToDouble(record.Credit);
                double amountD = Convert.ToDouble(record.Debit);

                bool useC = true;
                if (amountC > 0)
                {
                    dOrC = "C";
                }
                else if (amountD > 0)
                {
                    useC = false;
                    dOrC = "D";
                }

                string narrative = $"{record.Ref?.Trim()}";
                string c61 =
                    $"{valDateStr}{valDateStr2}{dOrC}R{(useC ? amountC.ToString("N2").Replace(",", "").Replace(".", ",") : amountD.ToString("N2").Replace(",", "").Replace(".", ","))}S205{narrative}";

                lines.AppendLine($":61:{c61}  {record.Details?.Trim()}");
            }

            lines.AppendLine(":62F:" +
                             $@"C{balDate:yyMMdd}{currency}{closingBal.ToString("N2").Replace(",", "").Replace(".", ",")}");

            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
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
            StringBuilder content = new StringBuilder();

            ReaderProperties readProps = new ReaderProperties().SetPassword(Encoding.Default.GetBytes(password));

            using (PdfReader reader = new PdfReader(path, readProps))
            {
                PdfDocument pdfDocument = new PdfDocument(reader);

                int pages = pdfDocument.GetNumberOfPages();

                for (int i = 1; i <= pages; i++)
                {
                    SimpleTextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                    PdfPage page = pdfDocument.GetPage(i);

                    string text = PdfTextExtractor.GetTextFromPage(page, strategy);

                    content.Append(text);
                }
            }

            return content.ToString();
        }

        public class ExtractedTableCRDB
        {
            public DateTime PostingDate { get; set; }
            public string Details { get; set; }
            public string Ref { get; set; }
            public DateTime ValueDate { get; set; }
            public string Debit { get; set; }
            public string Credit { get; set; }
            public string BookBalance { get; set; }
        }
    }
}