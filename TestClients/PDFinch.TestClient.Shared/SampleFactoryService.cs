using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PDFinch.Client.Common;

namespace PDFinch.TestClient.Shared
{
    /// <summary>
    /// Example usage in a hosted service, injecting an <see cref="IPdfClientFactory"/>.
    /// </summary>
    public class SampleFactoryService : PdfGeneratingServiceBase, IHostedService
    {
        private readonly IPdfClientFactory _pdfClientFactory;

        public SampleFactoryService(IPdfClientFactory pdfClientFactory)
        {
            _pdfClientFactory = pdfClientFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            IPdfClient orderClient = _pdfClientFactory.GetPdfClient("Production");

            await CreatePdfAsync(orderClient, "IPdfClientFactory-Production", cancellationToken);
        }
    }
}
