using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace CloudComputing.Services
{
    public class GeneratePdfService : IGeneratePdfService
    {
        public async Task<byte[]> GeneratePdfAsync(string redactedText)
        {
            using var memoryStream = new MemoryStream();
            var document = new PdfDocument();
            document.Info.Title = "Redacted PDF";

            const int maxCharactersPerLine = 80;
            var lines = WrapText(redactedText, maxCharactersPerLine);

            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 12);
            var yPosition = 0;

            const int leftMargin = 40;
            const int topMargin = 40;

            foreach (var line in lines)
            {
                gfx.DrawString(line, font, XBrushes.Black, new XRect(leftMargin, topMargin + yPosition, page.Width - 2 * leftMargin, page.Height), XStringFormats.TopLeft);
                yPosition += 14;
                if (topMargin + yPosition + 14 > page.Height)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPosition = 0;
                }
            }

            document.Save(memoryStream, false);
            return await Task.FromResult(memoryStream.ToArray());
        }

        private List<string> WrapText(string text, int maxCharactersPerLine)
        {
            var wrappedLines = new List<string>();
            var words = text.Split(' ');
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 > maxCharactersPerLine)
                {
                    wrappedLines.Add(currentLine.ToString());
                    currentLine.Clear();
                }
                if (currentLine.Length > 0)
                {
                    currentLine.Append(" ");
                }
                currentLine.Append(word);
            }

            if (currentLine.Length > 0)
            {
                wrappedLines.Add(currentLine.ToString());
            }

            return wrappedLines;
        }
    }
}
