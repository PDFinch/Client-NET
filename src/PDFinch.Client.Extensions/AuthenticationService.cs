using System.Collections.Concurrent;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PDFinch.Client.Common;

[assembly: InternalsVisibleTo("PDFinch.Client.Extensions.Tests")]
namespace PDFinch.Client.Extensions
{
    internal class AuthenticationService : IAuthenticationService
    {
        private static readonly ConcurrentDictionary<string, ResponseToken> ResponseTokenCache = new();

        private readonly IHttpClientFactory _clientFactory;
        private readonly PdfClientOptionsList _pdfinchOptions;

        public AuthenticationService(IHttpClientFactory clientFactory, PdfClientOptionsList pdfinchOptionList)
        {
            _clientFactory = clientFactory;
            _pdfinchOptions = pdfinchOptionList;
        }

        public async Task<ResponseToken> GetTokenAsync(string apiKey)
        {
            if (ResponseTokenCache.TryGetValue(apiKey, out var cachedToken) && cachedToken is { IsExpired: false })
            {
                return cachedToken;
            }
            
            var pdfinchSettings = _pdfinchOptions.GetClientOptions(apiKey);

            using var authClient = _clientFactory.CreateClient(Resources.AuthenticationClientID);

            authClient.SetUserAgentToAssemblyVersion();

            authClient.BaseAddress = pdfinchSettings.GetBaseUrl();

            var newToken = await authClient.GetTokenAsync(pdfinchSettings.ApiKey, pdfinchSettings.ApiSecret);

            _ = ResponseTokenCache.AddOrUpdate(apiKey, newToken, (_, _) => newToken);
            
            return newToken;
        }
    }
}
