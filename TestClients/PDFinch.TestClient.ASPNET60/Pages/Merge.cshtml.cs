using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDFinch.Client.Common;

namespace PDFinch.TestClient.ASPNET60.Pages
{
    public class MergeModel : PageModel
    {
        private readonly IPdfClientFactory _pdfClientFactory;

        public MergeModel(IPdfClientFactory pdfClientFactory)
        {
            _pdfClientFactory = pdfClientFactory;
        }

        public string? StatusMessage { get; private set; }

        public async Task<IActionResult> OnGet()
        {
            // Use the factory to request a named client:
            var client = _pdfClientFactory.GetPdfClient("Develop");

            var requests = new List<PdfRequest>
            {
                new ($"<h1>Portrait</h1><p>Generated on {DateTime.Now:F}.</p>")
                {
                    Landscape = false,
                    MarginTop = 42,
                    MarginBottom = 41,
                    MarginLeft = 40,
                    MarginRight = 39,
                },
                new ($"<h1>Landscape</h1><p>Generated on {DateTime.Now:F}.</p>")
                {
                    Landscape = true,
                    GrayScale = true,
                    
                },
                new ($"<h1>Portrait</h1><h2>(again)</h2><p>Generated on {DateTime.Now:F}.</p>")
                {
                    Landscape = false
                },
            };

            var pdfResult = await client.GenerateMergedPdfFromHtmlAsync(requests);

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
