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
            var content = new StringBuilder();

            var readProps = new ReaderProperties().SetPassword(Encoding.Default.GetBytes(password));

            using (var reader = new PdfReader(path, readProps))
            {
                var pdfDocument = new PdfDocument(reader);

                var pages = pdfDocument.GetNumberOfPages();

                for (var i = 1; i <= pages; i++)
                {
                    var strategy = new LocationTextExtractionStrategy();

                    var page = pdfDocument.GetPage(i);

                    var text = PdfTextExtractor.GetTextFromPage(page, strategy);

                    content.Append(text);
                }
            }

            return content.ToString();
        }
    }
}