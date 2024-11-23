using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace CloudComputing.Controllers
{
    [ApiController]
    [Route("api/getPdf")]
    public class PdfController : ControllerBase
    {
        [HttpPost]
        public IActionResult GetPdf([FromBody] string text)
        {
            string redactedText = RedactSensitiveInformation(text);
            byte[] pdfData = GeneratePdf(redactedText);

            return File(pdfData, "application/pdf", "redacted.pdf");
        }

        private string RedactSensitiveInformation(string text)
        {
            var namePattern = @"\b[A-Z][a-z]+\s[A-Z][a-z]+\b"; 
            var emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
            var addressPattern = @"\d{1,5}\s\w+(\s\w+)*,\s\w+(\s\w+)*,\s[A-Z]{2}\s\d{5}";

            text = Regex.Replace(text, namePattern, "[REDACTED]");
            text = Regex.Replace(text, emailPattern, "[REDACTED]");
            text = Regex.Replace(text, addressPattern, "[REDACTED]");

            return text;
        }

        private byte[] GeneratePdf(string redactedText)
        {
            using (var memoryStream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                document.Add(new Paragraph(redactedText));

                document.Close();
                return memoryStream.ToArray();
            }
        }
    }
}
