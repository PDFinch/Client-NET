using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PDFinch.Client.Common;

namespace PDFinch.TestClient.Shared
{
    /// <summary>
    /// Example usage in a hosted service, injecting an <see cref="IPdfClient"/>.
    /// </summary>
    public class SampleClientService : PdfGeneratingServiceBase, IHostedService
    {
        private readonly IPdfClient _pdfClient;

        public SampleClientService(IPdfClient pdfClient)
        {
            _pdfClient = pdfClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await CreatePdfAsync(_pdfClient, "Typed-IPdfClient", cancellationToken);
        }
    }
}
