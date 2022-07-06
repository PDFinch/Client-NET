using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace PDFinch.Client.Common
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Configures the User-Agent request header to contain this assembly's version.
        /// </summary>
        public static void SetUserAgentToAssemblyVersion(this HttpClient authClient)
        {
            var assemblyVersion = FileVersionInfo.GetVersionInfo(typeof(HttpClientExtensions).Assembly.Location);

            var userAgent = new ProductInfoHeaderValue(new ProductHeaderValue("PDFinch-Client-NET", assemblyVersion.ProductVersion));

            authClient.DefaultRequestHeaders.UserAgent.Clear();
            authClient.DefaultRequestHeaders.UserAgent.Add(userAgent);
        }

        /// <summary>
        /// Authenticates using the given client, clientId and clientSecret. Throws when that doesn't succeed.
        /// </summary>
        public static async Task<ResponseToken> GetTokenAsync(this HttpClient authClient, string clientId, string clientSecret)
        {
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("grant_type", "client_credentials")
            });

            var result = await authClient.PostAsync(Resources.OAuth2Endpoint, content);

            result.EnsureSuccessStatusCode();

            var jsonString = await result.Content.ReadAsStringAsync();

            ResponseToken? responseToken;

            try
            {
                responseToken = JsonSerializer.Deserialize<ResponseToken>(jsonString);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not parse token JSON for client_id '{clientId}': {jsonString}", ex);
            }

            if (responseToken == null)
            {
                throw new InvalidOperationException($"Could not obtain token for client_id '{clientId}', JSON: {jsonString}");
            }

            if (responseToken.IsExpired)
            {
                var tokenTimeErrorString = $"Token for client '{clientId}' came in expired, now: {DateTimeOffset.Now}, expires_in: {responseToken.ExpiresIn}s, skew: {Resources.DefaultClockSkewInSeconds}s";
                throw new InvalidOperationException(tokenTimeErrorString);
            }

            return responseToken;
        }
    }
}
