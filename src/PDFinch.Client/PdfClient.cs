using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PDFinch.Client.Common;

[assembly: InternalsVisibleTo("PDFinch.Client.Tests")]
namespace PDFinch.Client
{
    internal class PdfClient : HttpPdfClient
    {
        private readonly string _clientId;
        private readonly string _clientSecret;

        private readonly DateTime _created;

        private readonly HttpClient _client;

        private ResponseToken? _responseToken;

        private readonly TimeSpan _maxClientLifeTime;
        private readonly TimeSpan _defaultClientLifeTime = TimeSpan.FromMinutes(60);

        internal PdfClient(HttpClient httpClient, PdfClientOptions options, TimeSpan? clientLifeTime = null)
            : base(options)
        {
            _client = httpClient;

            _clientId = options.ApiKey;
            _clientSecret = options.ApiSecret;

            _created = DateTime.Now;
            _maxClientLifeTime = clientLifeTime ?? _defaultClientLifeTime;
        }

        protected override async Task<HttpClient> AuthenticateClientAsync(HttpRequestMessage httpRequestMessage)
        {
            if (_responseToken is { IsExpired: false })
            {
                return _client;
            }

            _client.DefaultRequestHeaders.Authorization = null;

            _responseToken = await _client.GetTokenAsync(_clientId, _clientSecret);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _responseToken.AccessToken);

            return _client;
        }

        internal bool IsExpired() => DateTime.Now - _created > _maxClientLifeTime;
    }
}
