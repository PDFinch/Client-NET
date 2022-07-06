using System.Collections.Generic;

namespace PDFinch.Client.Common
{
    /// <summary>
    /// Represents the configuration system entry.
    /// </summary>
    // ReSharper disable once InconsistentNaming - We're called "PDFinch".
    public class PDFinchClientSettings : PdfClientOptions
    {
        /// <summary>
        /// The default configuration section name we try to read.
        /// </summary>
        public const string ConfigurationSectionName = "PDFinch";

        /// <summary>
        /// The list of configured clients, if any.
        /// </summary>
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global - Initialized by configuration system.
        public IList<PdfClientOptions>? Clients { get; set; } = new List<PdfClientOptions>();
    }
}
