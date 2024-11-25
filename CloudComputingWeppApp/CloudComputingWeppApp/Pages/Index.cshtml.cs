using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CloudComputingWeppApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string InputText { get; set; }

        public async Task<IActionResult> OnPostSubmitAsync()
        {
            if (!string.IsNullOrEmpty(InputText))
            {
                var client = _httpClientFactory.CreateClient("RedactionService");
                var jsonContent = new StringContent($"{{\"text\": \"{InputText}\"}}", Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/getPdf", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var pdfData = await response.Content.ReadAsByteArrayAsync();
                    return File(pdfData, "application/pdf", "redacted.pdf");
                }
            }

            return Page();
        }
    }
}
