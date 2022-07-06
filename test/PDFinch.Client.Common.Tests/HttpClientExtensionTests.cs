using System;
using System.Net.Http;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using PDFinch.Client.Tests.Shared;

namespace PDFinch.Client.Common.Tests
{
    public class HttpClientExtensionTests
    {
        [Test]
        public async Task GetTokenAsync_Parses_Json()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            };

            var (handler, client) = SetUpAuthResponse(TestConstants.AuthResponse);

            // Act
            var token = await client.GetTokenAsync(options.ApiKey, options.ApiSecret);

            // Assert
            Assert.IsNotNull(token);
            Assert.IsFalse(token.IsExpired);
            Assert.AreEqual(42, token.ExpiresIn);

            handler.VerifyAll();
        }

        [Test]
        public void GetTokenAsync_Handles_InvalidJson()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            };

            var (handler, client) = SetUpAuthResponse(TestConstants.InvalidJsonResponse);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await client.GetTokenAsync(options.ApiKey, options.ApiSecret));

            handler.VerifyAll();
        }

        [Test]
        public void GetTokenAsync_Handles_EmptyResponse()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            };

            var (handler, client) = SetUpAuthResponse(TestConstants.EmptyJsonResponse);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await client.GetTokenAsync(options.ApiKey, options.ApiSecret));

            handler.VerifyAll();
        }

        [Test]
        public void GetTokenAsync_Handles_NullResponse()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            };

            var (handler, client) = SetUpAuthResponse(TestConstants.NullJsonResponse);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await client.GetTokenAsync(options.ApiKey, options.ApiSecret));

            handler.VerifyAll();
        }

        private static (Mock<HttpMessageHandler> handler, HttpClient httpClient) SetUpAuthResponse(HttpResponseMessage jsonResponse)
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handler.MockResponse((request, _) =>
                request.RequestUri?.AbsolutePath == "/" + Resources.OAuth2Endpoint
                    ? jsonResponse
                    : throw new InvalidOperationException(nameof(request.RequestUri)));

            var client = new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("https://throw-when-called")
            };

            return (handler, client);
        }
    }
}