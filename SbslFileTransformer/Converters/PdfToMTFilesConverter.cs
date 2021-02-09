using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace SbslFileTransformer.Converters
{
    public class PdfToMTFilesConverter
    {
        public void ConvertFile(string inputFile, string password = "", string outputFile = null)
        {
            var text = GetTextFromPdf(inputFile, password);

            string bankAcc = string.Empty;
            var transactions = new List<ExtractedTableCRDB>();
            bool isNewTableLine = true;

            ExtractedTableCRDB extractedTableLine = null;

            bool areTableValues = false;

            foreach (var line in text.Split('\n', '\r'))
            {
                if (line.Contains("Posting Date"))
                {
                    areTableValues = true;
                }

                //FOR CRDB
                if (line.ToLower().Contains("account:"))
                {
                    bankAcc = line.Split(":")[1].Trim();
                }

                if (areTableValues && Regex.IsMatch(line.Trim(), @"^\d{2}\.\d{2}\.\d{4}$") && isNewTableLine)
                {
                    extractedTableLine = new ExtractedTableCRDB();
                    transactions.Add(extractedTableLine);//-----------------------------------------===================
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

                    if (2 < split.Length)
                    {
                        extractedTableLine.Ref += split[2].Trim();
                    }
                    continue;
                }

                if (areTableValues && !line.Contains("Posting Date") && !line.ToLower().Contains("ref:") && !Regex.IsMatch(line.Trim(), @"^\d{2}\.\d{2}\.\d{4}$")
                    && !Regex.IsMatch(line.Trim(), @"^\d{2}\:\d{2}\:\d{2}$") && !Regex.IsMatch(line.Trim(), @"\d{1,3}(,\d{3})*(\.\d+)?"))
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

                if (areTableValues && Regex.IsMatch(line.Trim(), @"\d{1,3}(,\d{3})*\.\d{2}?$"))
                {
                    var numbers = line.Trim().Split(' ');
                    extractedTableLine.Debit = numbers[0];
                    extractedTableLine.Credit = numbers[1];
                    extractedTableLine.BookBalance = numbers[2];

                    isNewTableLine = true;
                    continue;
                }
            }

            StringBuilder lines = new StringBuilder();

            lines.AppendLine(":20:" + "1");
            lines.AppendLine(":25:" + "1/1");
            lines.AppendLine(":28C:" + bankAcc);

            foreach (var record in transactions)
            {

            }
        }


        public static string GetTextFromPdf(string path, string password = "")
        {
            var content = new StringBuilder();

            var readProps = new ReaderProperties().SetPassword(Encoding.Default.GetBytes(password));

            using (PdfReader reader = new PdfReader(path, readProps))
            {
                var pdfDocument = new PdfDocument(reader);

                var pages = pdfDocument.GetNumberOfPages();

                for (int i = 1; i <= pages; i++)
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