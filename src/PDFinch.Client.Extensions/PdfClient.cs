using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PDFinch.Client.Common;

namespace PDFinch.Client.Extensions
{
    internal class PdfClient : HttpPdfClient
    {
        private readonly HttpClient _httpClient;

        internal PdfClient(HttpClient httpClient, PdfClientOptions pdfinchOptions)
            : base(pdfinchOptions)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            // In most calls, this will be negligible, but it'll clean our DI config cache upon first creation call.
            DependencyInjection.ClearConfigurationCache();
        }

        protected override Task<HttpClient> AuthenticateClientAsync(HttpRequestMessage httpRequestMessage)
        {
            // Save the API key on the request, so the AuthenticationHandler can find or request a token for it.
            httpRequestMessage.Properties.Add(new KeyValuePair<string, object?>("apikey", ApiKey));

            // This client may not be authorized yet, but it will be by its handler.
            return Task.FromResult(_httpClient);
        }
    }
}
