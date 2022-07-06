using System;

namespace PDFinch.Client.Common
{
    /// <summary>
    /// Contains client configuration values.
    /// </summary>
    // ReSharper disable PropertyCanBeMadeInitOnly.Global - intentioned to be set by callers
    public class PdfClientOptions
    {
#pragma warning disable CS8618 // We check those during configuration/startup.
        /// <summary>
        /// The API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The API secret.
        /// </summary>
        public string ApiSecret { get; set; }
#pragma warning restore CS8618

        /// <summary>
        /// Optional name for the client.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Which environment this client should talk to. Defaults to <see cref="PdfEnvironment.Production"/>.
        /// </summary>
        public PdfEnvironment Environment { get; set; } = PdfEnvironment.Production;
        
        /// <summary>
        /// Optional base URL for testing purposes.
        /// </summary>
        public Uri? BaseUrl { get; set; }

        /// <summary>
        /// Gets the base URL this client will talk to.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Uri GetBaseUrl()
        {
            return Environment switch
            {
                PdfEnvironment.Production => Resources.ProductionBaseUri,
                PdfEnvironment.Staging => Resources.StagingBaseUri,
                PdfEnvironment.Custom => BaseUrl ?? throw new ArgumentNullException(nameof(BaseUrl)),
                _ => throw new ArgumentException($"Unknown environment '{Environment}'", nameof(Environment))
            };
        }

        /// <summary>
        /// Enables Compression <see cref="System.Net.DecompressionMethods.GZip"/> and <see cref="System.Net.DecompressionMethods.Deflate"/> where possible. Defaults to <see langword="false"/>.
        /// </summary>
        public bool EnableCompression { get; set; } = false;
    }
}
