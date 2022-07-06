using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PDFinch.Client.Common;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace PDFinch.Client.Extensions
{
    internal class AuthenticationHandler : DelegatingHandler
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationHandler(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Properties.TryGetValue(Resources.HttpRequestApiKeyOption, out var apikey) && apikey is string apikeyString)
            {
                var responseToken = await _authenticationService.GetTokenAsync(apikeyString);

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", responseToken.AccessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
