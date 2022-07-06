using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDFinch.Client.Common;

namespace PDFinch.TestClient.ASPNET60.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IPdfClientFactory _pdfClientFactory;
        //private readonly IPdfClient _pdfClient;

        public IndexModel(IPdfClientFactory pdfClientFactory/*, IPdfClient pdfClient*/)
        {
            _pdfClientFactory = pdfClientFactory;
        }

        public string? StatusMessage { get; private set; }

        public async Task<IActionResult> OnGet()
        {
            // TODO: separate page for DI
            //// Typed client (IPdfClient in constructor)
            //var pdfResult = await _pdfClient.GeneratePdfFromHtmlAsync($"<h1>Typed IPdfClient</h1><p>Generated on {DateTime.Now:F}.");
            
            //if (!pdfResult.Success)
            //{
            //    StatusMessage = pdfResult.StatusMessage;

            //    return Page();
            //}

            // Use the factory to request a named client:
            var client = _pdfClientFactory.GetPdfClient("Develop");

            var pdfResult = await client.GeneratePdfFromHtmlAsync($"<h1>Everything works!</h1><p>This is IHttpClientFactory.GetPdfClient(\"Develop\")'s output.</p><p>Generated on {DateTime.Now:F}.");
            
            // Returning that PDF when it succeeded.
            if (pdfResult.Success)
            {
                return File(pdfResult.Data, "application/pdf");
            }

            StatusMessage = pdfResult.StatusMessage;

            return Page();
        }
    }
}
