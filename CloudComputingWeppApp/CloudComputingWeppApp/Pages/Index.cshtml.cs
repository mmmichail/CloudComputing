using System.Text;
using CloudComputingWeppApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace CloudComputingWeppApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        [BindProperty]
        public string InputText { get; set; }
        [BindProperty]
        public string GeneratedHash { get; set; }
        [BindProperty]
        public string InputHash { get; set; }
        [BindProperty]
        public List<string> PdfHashedList { get; set; }

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostCreatePdfAsync()
        {
            if (!string.IsNullOrEmpty(InputText))
            {
                var client = _httpClientFactory.CreateClient("RedactionService");
                var jsonContent = new StringContent($"{{\"text\": \"{InputText}\"}}", Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Pdf/CreatePdf", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var redactionResponse = await response.Content.ReadFromJsonAsync<RedactionResponse>();
                    GeneratedHash = redactionResponse?.Hash;
                }
            }

            return Page();
        }


        public async Task<IActionResult> OnPostGetPdfByHashAsync()
        {
            if (!string.IsNullOrEmpty(InputHash))
            {
                var client = _httpClientFactory.CreateClient("RedactionService");
                var response = await client.GetAsync($"api/Pdf/GetPdf/{InputHash}");

                if (response.IsSuccessStatusCode)
                {
                    var pdfData = await response.Content.ReadAsByteArrayAsync();
                    return File(pdfData, "application/pdf", "redacted.pdf");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to retrieve PDF. Please check the hash.";
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostGetHashListAsync()
        {
            var client = _httpClientFactory.CreateClient("RedactionService");
            var response = await client.GetAsync($"api/Pdf/ListPdfs");

            if (response.IsSuccessStatusCode)
            {
                PdfHashedList = await response.Content.ReadFromJsonAsync<List<string>>();
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to retrieve Hash List";
            }

            return Page();
        }
    }
}
