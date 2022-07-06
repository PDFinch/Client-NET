using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PDFinch.Client.Common.Tests
{
    public class BadPdfClient : HttpPdfClient
    {
        public BadPdfClient(PdfClientOptions pdfinchOptions) : base(pdfinchOptions) { }

        protected override Task<HttpClient> AuthenticateClientAsync(HttpRequestMessage httpRequestMessage)
        {
            throw new InvalidOperationException("Tis but a test");
        }
    }
}
