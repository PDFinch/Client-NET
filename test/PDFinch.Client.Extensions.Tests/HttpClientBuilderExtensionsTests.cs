using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PDFinch.Client.Common;
using PDFinch.Client.Tests.Shared;

#pragma warning disable CS8509 // Assert count, not in switch

namespace PDFinch.Client.Extensions.Tests
{
    internal class HttpClientBuilderExtensionsTests
    {
        [Test]
        public async Task ConfigureAuthClientHandler_On500Error_Retries_Once()
        {
            // Arrange
            var handlerMock = new Mock<DelegatingHandler>(MockBehavior.Strict);

            int callCount = 0;
            handlerMock.MockResponse((_, _) => new HttpResponseMessage(HttpStatusCode.InternalServerError)).Callback(() => callCount++);

            var services = new ServiceCollection();

            services.AddHttpClient(Resources.AuthenticationClientID)
                    // Test this one
                    .ConfigureAuthClientHandler()
                    .AddHttpMessageHandler(() => handlerMock.Object);

            var client = services.BuildServiceProvider()
                                 .GetRequiredService<IHttpClientFactory>()
                                 .CreateClient(Resources.AuthenticationClientID);

            // Act
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri("https://throw-when-called")));

            // Assert
            Assert.AreEqual(500, (int)response.StatusCode);
            Assert.AreEqual(2, callCount);
        }

        [Test]
        public async Task ConfigureApiClientHandler_On401_Authenticates()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "client-01",
                ApiSecret = "secret-01",
                Environment = PdfEnvironment.Custom,
                BaseUrl = new Uri("https://client-01-base-url")

            };
            var responseToken = new ResponseToken
            {
                AccessToken = "eyj123",
                ExpiresIn = 42,
                TokenType = "Tolkien",
            };

            var targetEndpoint = new Uri("https://throw-when-called/create-pdf");

            var handlerMock = new Mock<DelegatingHandler>(MockBehavior.Strict);

            int callCount = 0;

            handlerMock.MockResponse((request, _) => ++callCount switch
            {
                // The first call isn't logged in, client should try to log in next.
                1 => request.RequestUri == targetEndpoint ? new HttpResponseMessage(HttpStatusCode.Unauthorized) : throw new ArgumentException(),

                // Client retries original call, now gets a 500.
                2 => request.RequestUri == targetEndpoint ? new HttpResponseMessage(HttpStatusCode.InternalServerError) : throw new ArgumentException(),
                
                // Retries it once more, gets OK.
                3 => request.RequestUri == targetEndpoint ? new HttpResponseMessage(HttpStatusCode.OK) : throw new ArgumentException(),
            }).Verifiable();

            var services = new ServiceCollection();

            // We don't populate the request's apikey property here, so no auth happens (why?).
            var authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            authenticationServiceMock.Setup(a => a.GetTokenAsync(options.ApiKey)).ReturnsAsync(() => responseToken);

            services.AddTransient(_ => authenticationServiceMock.Object);
            services.AddTransient(s => new AuthenticationHandler(s.GetRequiredService<IAuthenticationService>()));

            services.AddHttpClient(Resources.AuthenticationClientID)
                    // Test this one
                    .ConfigureApiClientHandler(options)
                    .AddHttpMessageHandler(() => handlerMock.Object);

            var client = services.BuildServiceProvider()
                                 .GetRequiredService<IHttpClientFactory>()
                                 .CreateClient(Resources.AuthenticationClientID);

            var fakeMessage = new HttpRequestMessage(HttpMethod.Get, targetEndpoint);

            // Pretend that it went past the AuthenticationService.
#pragma warning disable CS0618 // We target .NET Core 2.
            fakeMessage.Properties.Add(new KeyValuePair<string, object?>(Resources.HttpRequestApiKeyOption, options.ApiKey));
#pragma warning restore CS0618

            // Act
            var response = await client.SendAsync(fakeMessage);

            // Assert
            Assert.AreEqual(200, (int)response.StatusCode);
            Assert.AreEqual(3, callCount);
        }
    }
}
