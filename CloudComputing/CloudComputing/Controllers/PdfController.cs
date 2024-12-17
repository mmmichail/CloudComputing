using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using CloudComputing.Models;
using System.Text;
using System.Security.Cryptography;

namespace CloudComputing.Controllers
{
    [ApiController]
    [Route("api/Pdf")]
    public class PdfController : ControllerBase
    {
        [HttpPost("CreatePdf")]
        public IActionResult CreatePdf([FromBody] RedactionRequest request)
        {
            if (string.IsNullOrEmpty(request?.Text))
            {
                return BadRequest("The text to redact is required.");
            }

            string hash = GenerateHash(request.Text);
            string pdfFilePath = Path.Combine("RedactedPdfs", $"{hash}.pdf");

            string redactedText = RedactSensitiveInformation(request.Text);
            byte[] pdfData = GeneratePdf(redactedText);

            Directory.CreateDirectory("RedactedPdfs");
            System.IO.File.WriteAllBytes(pdfFilePath, pdfData);

            var response = new RedactionResponse
            {
                Hash = hash,
                Links = new List<Link>
                {
                    new Link
                    {
                        Rel = "get-pdf",
                        Href = Url.Action(nameof(GetPdfByHash), "Pdf", new { hash }, Request.Scheme)
                    }
                }
            };

            return Ok(response);
        }

        [HttpGet("GetPdf/{hash}")]
        public IActionResult GetPdfByHash(string hash)
        {
            string pdfFilePath = Path.Combine("RedactedPdfs", $"{hash}.pdf");

            if (!System.IO.File.Exists(pdfFilePath))
            {
                return NotFound("The requested PDF was not found.");
            }

            byte[] pdfData = System.IO.File.ReadAllBytes(pdfFilePath);
            return File(pdfData, "application/pdf", $"{hash}.pdf");
        }

        [HttpGet("ListPdfs")]
        public IActionResult ListPdfs()
        {
            string directoryPath = "RedactedPdfs";

            if (!Directory.Exists(directoryPath))
            {
                return Ok(new List<string>());
            }

            var pdfFiles = Directory.GetFiles(directoryPath, "*.pdf")
                                    .Select(Path.GetFileNameWithoutExtension)
                                    .ToList();

            return Ok(pdfFiles);
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

        private string GenerateHash(string text)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
