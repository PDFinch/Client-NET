using System;

namespace PDFinch.Client.Common
{
    /// <summary>
    /// Strings and such.
    /// </summary>
    public static class Resources
    {
        /// <summary>
        /// Production base URL.
        /// </summary>
        public static readonly Uri ProductionBaseUri = new("https://api.pdfinch.com/");

        /// <summary>
        /// Staging base URL.
        /// </summary>
        public static readonly Uri StagingBaseUri = new("https://api-staging.pdfinch.com/");

        /// <summary>
        /// Endpoint for client authentication.
        /// </summary>
        public const string OAuth2Endpoint = "oauth2/token";

        /// <summary>
        /// Endpoint for creating a PDF.
        /// </summary>
        public const string CreatePdfEndpoint = "pdf/create";

        /// <summary>
        /// HttpClient name for authentication clients.
        /// </summary>
        // ReSharper disable once InconsistentNaming - ID, not Id.
        public const string AuthenticationClientID = "pdfinch-auth";

        /// <summary>
        /// Number of seconds before (-) or after (+) the current time that the token will be considered expired in.
        /// </summary>
        public const int DefaultClockSkewInSeconds = -30;

        /// <summary>
        /// The property/option bag value in the HttpRequestMessage we use to signal which API key it belongs to.
        /// </summary>
        public const string HttpRequestApiKeyOption = "apikey";
    }
}
