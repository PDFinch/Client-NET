using System;
using System.Net.Http;
using PDFinch.Client.Common;

namespace PDFinch.Client.Extensions
{
    /// <summary>
    /// This factory is used with dependency injection.
    /// </summary>
    internal class PdfClientFactory : IPdfClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PdfClientOptionsList _options;

        /// <param name="httpClientFactory"></param>
        /// <param name="options"></param>
        public PdfClientFactory(IHttpClientFactory httpClientFactory, PdfClientOptionsList options)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public IPdfClient GetPdfClient(string? nameOrApiKey = null)
        {
            var clientOptions = _options.GetClientOptions(nameOrApiKey);

            var httpClient = _httpClientFactory.CreateClient(clientOptions.ApiKey);
            
            return new PdfClient(httpClient, clientOptions);
        }
    }
}
