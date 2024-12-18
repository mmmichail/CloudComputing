using Microsoft.AspNetCore.Mvc;
using CloudComputing.Models;
using CloudComputing.Services;

namespace CloudComputing.Controllers
{
    [ApiController]
    [Route("api/Pdf")]
    public class PdfController : ControllerBase
    {
        private readonly IRedactionService _redactionService;
        private readonly IGeneratePdfService _generatePdfService;
        private readonly ILogger<PdfController> _logger;

        public PdfController(
            IRedactionService redactionService,
            IGeneratePdfService generatePdfService,
            ILogger<PdfController> logger)
        {
            _redactionService = redactionService;
            _generatePdfService = generatePdfService;
            _logger = logger;
        }

        [HttpPost("CreatePdf")]
        public async Task<IActionResult> CreatePdf([FromBody] RedactionRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request?.Text))
            {
                _logger.LogWarning("Invalid CreatePdf request received: {Request}", request);
                return BadRequest("The text to redact is required.");
            }

            try
            {
                _logger.LogInformation("Received request to create PDF for text: {Text}", request.Text);

                string hash = GenerateHash(request.Text);
                string pdfFilePath = Path.Combine("RedactedPdfs", $"{hash}.pdf");

                string redactedText = _redactionService.RedactSensitiveInformation(request.Text);
                byte[] pdfData = await _generatePdfService.GeneratePdfAsync(redactedText);

                Directory.CreateDirectory("RedactedPdfs");
                await System.IO.File.WriteAllBytesAsync(pdfFilePath, pdfData);

                _logger.LogInformation("PDF successfully created and saved at: {FilePath}", pdfFilePath);

                var response = new RedactionResponse
                {
                    Hash = hash,
                    Links = new List<Link>
                    {
                        new Link
                        {
                            Rel = "get-pdf",
                            Href = Url.Action(nameof(GetPdfByHash), "Pdf", new { hash }, Request.Scheme)
                        },
                        new Link
                        {
                            Rel = "list-pdfs",
                            Href = Url.Action(nameof(ListPdfs), "Pdf", null, Request.Scheme)
                        }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating PDF.");
                return StatusCode(500, "An internal error occurred while processing the request.");
            }
        }

        [HttpGet("GetPdf/{hash}")]
        public async Task<IActionResult> GetPdfByHash(string hash)
        {
            try
            {
                string pdfFilePath = Path.Combine("RedactedPdfs", $"{hash}.pdf");

                if (!System.IO.File.Exists(pdfFilePath))
                {
                    _logger.LogWarning("PDF with hash {Hash} not found.", hash);
                    return NotFound("The requested PDF was not found.");
                }

                byte[] pdfData = await System.IO.File.ReadAllBytesAsync(pdfFilePath);

                var response = new
                {
                    Links = new List<Link>
                    {
                        new Link
                        {
                            Rel = "self",
                            Href = Url.Action(nameof(GetPdfByHash), "Pdf", new { hash }, Request.Scheme)
                        },
                        new Link
                        {
                            Rel = "list-pdfs",
                            Href = Url.Action(nameof(ListPdfs), "Pdf", null, Request.Scheme)
                        }
                    }
                };

                return File(pdfData, "application/pdf", $"{hash}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving PDF with hash {Hash}.", hash);
                return StatusCode(500, "An internal error occurred while processing the request.");
            }
        }

        [HttpGet("ListPdfs")]
        public IActionResult ListPdfs()
        {
            try
            {
                string directoryPath = "RedactedPdfs";

                if (!Directory.Exists(directoryPath))
                {
                    _logger.LogWarning("Redacted PDFs directory does not exist.");
                    return Ok(new { PdfFiles = new List<string>(), Links = GenerateDefaultLinks() });
                }

                var pdfFiles = Directory.GetFiles(directoryPath, "*.pdf")
                                        .Select(Path.GetFileNameWithoutExtension)
                                        .ToList();

                var response = new
                {
                    PdfFiles = pdfFiles,
                    Links = GenerateDefaultLinks()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while listing PDFs.");
                return StatusCode(500, "An internal error occurred while processing the request.");
            }
        }

        private string GenerateHash(string text)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hash;
            }
        }

        private List<Link> GenerateDefaultLinks()
        {
            return new List<Link>
            {
                new Link
                {
                    Rel = "create-pdf",
                    Href = Url.Action(nameof(CreatePdf), "Pdf", null, Request.Scheme)
                },
                new Link
                {
                    Rel = "list-pdfs",
                    Href = Url.Action(nameof(ListPdfs), "Pdf", null, Request.Scheme)
                }
            };
        }
    }
}
