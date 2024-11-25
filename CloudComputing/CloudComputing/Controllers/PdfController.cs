using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using CloudComputing.Models;

namespace CloudComputing.Controllers
{
    [ApiController]
    [Route("api/getPdf")]
    public class PdfController : ControllerBase
    {
        [HttpPost]
        public IActionResult GetPdf([FromBody] RedactionRequest request)
        {
            if (string.IsNullOrEmpty(request?.Text))
            {
                return BadRequest("The text to redact is required.");
            }

            string redactedText = RedactSensitiveInformation(request.Text);
            byte[] pdfData = GeneratePdf(redactedText);

            return File(pdfData, "application/pdf", "redacted.pdf");
        }

        private string RedactSensitiveInformation(string text)
        {
            var namePattern = @"\b[A-Z][a-z]+\s[A-Z][a-z]+\b"; 
            var emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
            var addressPattern = @"(\d{1,5}\s[\w\s]+|\w+\s\d+)(,\s[\w\s]+)*";

            text = Regex.Replace(text, namePattern, "[REDACTED]");
            text = Regex.Replace(text, emailPattern, "[REDACTED]");
            text = Regex.Replace(text, addressPattern, "[REDACTED]");

            return text;
        }

        private byte[] GeneratePdf(string redactedText)
        {
            using (var memoryStream = new MemoryStream())
            {
                PdfDocument document = new();
                document.Info.Title = "Redacted PDF";
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new("Verdana", 12);
                gfx.DrawString(redactedText, font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.TopLeft);
                document.Save(memoryStream, false);
                return memoryStream.ToArray();
            }
        }
    }
}
