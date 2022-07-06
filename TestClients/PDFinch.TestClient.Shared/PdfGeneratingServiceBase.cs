using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PDFinch.Client.Common;

namespace PDFinch.TestClient.Shared
{
    public class PdfGeneratingServiceBase
    {
        protected static async Task CreatePdfAsync(IPdfClient pdfClient, string titleAndFilename, CancellationToken cancellationToken)
        {
            PdfResult<Stream> pdfResult = await pdfClient.GeneratePdfFromHtmlAsync($"<h1>{titleAndFilename}</h1><p>Generated on {DateTime.Now:F}.</p>", new PdfOptions
            {
                MarginBottom = 42,
                MarginTop = 42,
                MarginLeft = 42,
                MarginRight = 42,
                Landscape = true,
                GrayScale = false,
            });

            if (pdfResult.Success)
            {
                // A file is just one way of storing the result...
                using var fileStream = File.Create($"Temp/PDFinch-{titleAndFilename}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.pdf");

                // ... you can copy it to any other stream.
                await pdfResult.Data!.CopyToAsync(fileStream, bufferSize: 81920, cancellationToken);
            }
            else
            {
                // TODO: handle, log orderPdfResult.StatusMessage
                throw new InvalidOperationException($"Error generating PDF: {pdfResult.StatusMessage}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
