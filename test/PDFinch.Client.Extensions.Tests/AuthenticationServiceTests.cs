using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PDFinch.Client.Common;
using Moq;
using Moq.Language.Flow;
using PDFinch.Client.Tests.Shared;

namespace PDFinch.Client.Extensions.Tests
{
    public class AuthenticationServiceTests
    {
#pragma warning disable CS8618 // SetUp() sets them up, they're never null.
        private List<PdfClientOptions> _options;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<HttpMessageHandler> _httpHandlerMock;
        private IReturnsResult<HttpMessageHandler> _handlerReturns;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            _options = new List<PdfClientOptions>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);

            // Loose for non-overridable Dispose() that gets called.
            _httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            _handlerReturns = _httpHandlerMock.SetupAuth();

            // Clear token cache between tests.
            GetHandlerCache().Clear();

            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))

                // For /oauth2/, return the auth client.
                .Returns(new Func<string, HttpClient>(name =>
                    name == Resources.AuthenticationClientID
                        ? new HttpClient(_httpHandlerMock.Object)
                        : throw new ArgumentException(null, nameof(name))));
        }

        [Test]
        public async Task GetTokenAsync_Calls_HttpClient()
        {
            // Arrange
            var options = SetupClient(new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            });

            var classUnderTest = new AuthenticationService(_httpClientFactoryMock.Object, new PdfClientOptionsList(_options));

            // Act
            var token = await classUnderTest.GetTokenAsync(options.ApiKey);

            // Assert
            Assert.NotNull(token);
            Assert.AreEqual(42, token.ExpiresIn);
            Assert.AreEqual("typ", token.TokenType);
            Assert.AreEqual("tok", token.AccessToken);

            _httpHandlerMock.Verify();
            _httpClientFactoryMock.Verify();
        }

        [Test]
        public async Task GetTokenAsync_Caches_Token()
        {
            // Arrange
            var options = SetupClient(new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            });

            var classUnderTest = new AuthenticationService(_httpClientFactoryMock.Object, new PdfClientOptionsList(_options));

            // Act
            var token1 = await classUnderTest.GetTokenAsync(options.ApiKey);
            var token2 = await classUnderTest.GetTokenAsync(options.ApiKey);

            // Assert
            Assert.NotNull(token1);
            Assert.IsTrue(ReferenceEquals(token1, token2));

            _httpHandlerMock.Verify();
            _httpClientFactoryMock.Verify();
        }

        [Test]
        public async Task GetTokenAsync_Dismisses_Expired_Token()
        {
            // Arrange
            var options = SetupClient(new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            });

            var classUnderTest = new AuthenticationService(_httpClientFactoryMock.Object, new PdfClientOptionsList(_options));

            // Act
            _ = await classUnderTest.GetTokenAsync(options.ApiKey);

            var cachedToken1 = GetHandlerCache()[options.ApiKey];
            
            // Invalidate
            cachedToken1.ExpiresIn = 0;

            _ = await classUnderTest.GetTokenAsync(options.ApiKey);

            var cachedToken2 = GetHandlerCache()[options.ApiKey];

            // Assert
            Assert.IsTrue(cachedToken1.IsExpired);
            Assert.IsFalse(cachedToken2.IsExpired);

            _httpHandlerMock.Verify();
            _httpClientFactoryMock.Verify();
        }

        [Test]
        public async Task GetTokenAsync_Sets_UserAgent()
        {
            // Arrange
            var options = SetupClient(new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            });

            // Assert
            _handlerReturns.Callback((HttpRequestMessage m, CancellationToken _) =>
            {
                Assert.IsTrue(m.Headers.UserAgent.FirstOrDefault()?.ToString().Contains("PDFinch"));
            }).Verifiable();

            var classUnderTest = new AuthenticationService(_httpClientFactoryMock.Object, new PdfClientOptionsList(_options));

            // Act
            _ = await classUnderTest.GetTokenAsync(options.ApiKey);

            // Assert
            _httpClientFactoryMock.Verify();
        }

        private PdfClientOptions SetupClient(PdfClientOptions options)
        {
            _options.Add(options);

            return options;
        }

        private static ConcurrentDictionary<string, ResponseToken> GetHandlerCache()
        {
            var cacheAccessor = typeof(AuthenticationService).GetField("ResponseTokenCache", BindingFlags.NonPublic | BindingFlags.Static);
            return (ConcurrentDictionary<string, ResponseToken>)cacheAccessor!.GetValue(null)!;
        }
    }
}
