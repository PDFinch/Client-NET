using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PDFinch.Client.Common;

namespace PDFinch.Client
{
    /// <summary>
    /// A naive implementation of a client factory.
    /// TODO: check (I)HttpClient(Factory) usage.
    /// </summary>
    public class PdfClientFactory : IPdfClientFactory
    {
        private readonly List<PdfClientOptions> _optionsCache = new();
        
        private PdfClientOptionsList? _optionsList;
        
        private readonly ConcurrentDictionary<string, PdfClient> _clientCache = new();

        /// <summary>
        /// Registers a PDF client (<see cref="IPdfClient"/>) accoring to the provided <paramref name="options"></paramref>.
        /// </summary>
        /// <param name="options"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void RegisterPdfClient(PdfClientOptions options)
        {
            if (string.IsNullOrEmpty(options.ApiKey))
            {
                throw new ArgumentNullException(nameof(options.ApiKey));
            }

            if (string.IsNullOrEmpty(options.ApiSecret))
            {
                throw new ArgumentNullException(nameof(options.ApiSecret));
            }

            _optionsCache.Add(options);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IPdfClient GetPdfClient(string? nameOrApiKey = null)
        {
            if (_optionsCache?.Any() != true)
            {
                throw new InvalidOperationException("There are no clients registered. Call RegisterPdfClient() before calling GetPdfClient().");
            }

            _optionsList ??= new PdfClientOptionsList(_optionsCache);

            var options = _optionsList.GetClientOptions(nameOrApiKey);

            if (!_clientCache.TryGetValue(options.ApiKey, out var client) || client.IsExpired())
            {
                // TODO: this leaves an HttpClient to get garbage collected, eventually. How does this hold up under load?
                client = _clientCache[options.ApiKey] = new PdfClient(CreateHttpClient(options.GetBaseUrl()), options);
            }

            return client;
        }

        private static HttpClient CreateHttpClient(Uri baseUri)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = baseUri,
            };

            httpClient.SetUserAgentToAssemblyVersion();

            return httpClient;
        }
    }
}
