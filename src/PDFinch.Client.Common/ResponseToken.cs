using System;
using System.Text.Json.Serialization;

namespace PDFinch.Client.Common
{
    /// <summary>
    /// OAuth 2 response.
    /// </summary>
    public class ResponseToken
    {
        private int _expiresIn;
        private DateTimeOffset _expiresAt;

        /// <summary>
        /// Token type.Currently only "bearer".
        /// </summary>
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        /// <summary>
        /// The token.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        /// <summary>
        /// Number of seconds this token will expire in, from obtaining it.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn
        {
            get => _expiresIn;
            set
            {
                _expiresIn = value;
                _expiresAt = DateTimeOffset.Now.AddSeconds(ExpiresIn + Resources.DefaultClockSkewInSeconds);
            }
        }

        /// <summary>
        /// Indicates whether this token is expired.
        /// </summary>
        public bool IsExpired => _expiresAt < DateTimeOffset.Now;
    }
}
