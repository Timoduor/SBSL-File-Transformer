using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public class SbslPdfReader
    {

        public static string GetTextFromPDF(string path)
        {
            StringBuilder content = new StringBuilder();

            using (PdfReader reader = new PdfReader(path))
            {
                var pdfDocument = new PdfDocument(reader);

                var pages = pdfDocument.GetNumberOfPages();

                for (int i = 1; i <= pages; i++)
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
