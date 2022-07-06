using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using PDFinch.Client.Common;

namespace PDFinch.Client.Extensions
{
    /// <summary>
    /// Used to add additional PDFinch services to the collection.
    ///
    /// TODO: later make public and return the builder itself, so users can call .AddTypedClient&lt;MyCoolInvoiceService&gt;("Invoice");
    /// </summary>
    // ReSharper disable once InconsistentNaming - We're called "PDFinch".
    internal class PDFinchBuilder
    {
        private readonly PdfClientOptionsList _clientOptions;

        public IServiceCollection Services { get; }

        internal PDFinchBuilder(IServiceCollection services, PdfClientOptionsList clientOptions)
        {
            Services = services;
            _clientOptions = clientOptions;
        }

        /// <summary>
        /// Enable injection of <see cref="IPdfClient"/> for all types.
        /// 
        /// Keep internal, we always call it in AddPDFinch().
        /// </summary>
        internal PDFinchBuilder AddPdfClient(string? nameOrApiKey = null)
        {
            var options = _clientOptions.GetClientOptions(nameOrApiKey);

            ConfigureHttpClient(options);

            Services.AddScoped<IPdfClient>(s =>
            {
                var httpClient = s.GetRequiredService<IHttpClientFactory>().CreateClient(options.ApiKey);

                return new PdfClient(httpClient, options);
            });

            return this;
        }

#pragma warning disable CS1574, CS0419 // Ambiguous reference in cref attribute
        /// <summary>
        /// Registers the <see cref="IPdfClientFactory"/> with the configuration provided to <see cref="DependencyInjection.AddPDFinch"/>.
        ///
        /// Keep internal, we always call it in AddPDFinch().
        /// </summary>
#pragma warning restore CS1574, CS0419 // Ambiguous reference in cref attribute
        internal PDFinchBuilder AddPdfClientFactory()
        {
            // This obtains one or more user-registered, validated client configurations.
            var allOptions = _clientOptions.GetAll();

            Services.AddScoped<IPdfClientFactory, PdfClientFactory>(s => new PdfClientFactory(s.GetRequiredService<IHttpClientFactory>(), _clientOptions));

            foreach (var options in allOptions)
            {
                ConfigureHttpClient(options);
            }

            return this;
        }

        private void ConfigureHttpClient(PdfClientOptions options)
        {
            Services.AddHttpClient(options.ApiKey, httpClient =>
                {
                    httpClient.SetUserAgentToAssemblyVersion();

                    httpClient.BaseAddress = options.GetBaseUrl();

                    if (options.EnableCompression)
                    {
                        httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                        httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "deflate");
                    }
                }).ConfigurePrimaryHttpMessageHandler(messageHandler =>
                {
                    var handler = new HttpClientHandler();

                    if (options.EnableCompression)
                    {
                        if (handler.SupportsAutomaticDecompression)
                        {
                            handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
                        }
                    }
                    return handler;

                })
                .ConfigureApiClientHandler(options);
        }
    }
}
