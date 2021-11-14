using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public class SbslPdfReader
    {
        public static string GetTextFromPDF(string path, string password = "")
        {
            StringBuilder content = new StringBuilder();

            ReaderProperties readProps = new ReaderProperties().SetPassword(Encoding.Default.GetBytes(password));

            using (PdfReader reader = new PdfReader(path, readProps))
            {
                PdfDocument pdfDocument = new PdfDocument(reader);

                int pages = pdfDocument.GetNumberOfPages();

                for (int i = 1; i <= pages; i++)
                {
                    LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();

                    PdfPage page = pdfDocument.GetPage(i);

                    string text = PdfTextExtractor.GetTextFromPage(page, strategy);

                    content.Append(text);
                }
            }

            return content.ToString();
        }
    }
}