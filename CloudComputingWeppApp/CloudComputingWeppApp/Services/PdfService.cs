using Microsoft.AspNetCore.Mvc;

namespace CloudComputingWeppApp.Services
{
    public class PdfService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PdfService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostSubmitAsync(string inputText)
        {
            var client = _httpClientFactory.CreateClient("RedactionService");
            var response = await client.PostAsync("api/redact", new StringContent(inputText));

            if (response.IsSuccessStatusCode)
            {
                var pdfData = await response.Content.ReadAsByteArrayAsync();
                return File(pdfData, "application/pdf", "redacted.pdf");
            }

            return Page();
        }
    }
}
