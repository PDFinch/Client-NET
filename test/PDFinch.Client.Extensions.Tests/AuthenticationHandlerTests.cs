using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Moq;
using NUnit.Framework;
using PDFinch.Client.Common;
using PDFinch.Client.Tests.Shared;

namespace PDFinch.Client.Extensions.Tests
{
    public class AuthenticationHandlerTests
    {
        private string? _apiKey;
        private HttpClient? _httpClient;
        private HttpRequestMessage? _fakeMessage;
        private AuthenticationHandler? _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _apiKey = "api-01";
            var token = new ResponseToken
            {
                ExpiresIn = 42,
            };
            
            // The handler calls the IAuthenticationService
            var authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            authenticationServiceMock.Setup(a => a.GetTokenAsync(_apiKey)).ReturnsAsync(token);

            // The handler calls its inner handler, this one (that doesn't actually issue an outgoing call).
            var okayHandlerMock = new Mock<DelegatingHandler>(MockBehavior.Strict);
            okayHandlerMock.MockResponse((_, _) => new HttpResponseMessage(HttpStatusCode.OK));

            // Now we can set up 
            _classUnderTest = new AuthenticationHandler(authenticationServiceMock.Object)
            {
                InnerHandler = okayHandlerMock.Object
            };
        }

        [Test]
        public void SendAsync_()
        {
            // Arrange
            _httpClient = new HttpClient(_classUnderTest!)
            {
                BaseAddress = new Uri("https://throw-when-called")
            };

            _fakeMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("/", UriKind.Relative)){};

            // Pretend that it went past the AuthenticationService.
#pragma warning disable CS0618 // We target .NET Core 2.
            _fakeMessage.Properties.Add(new KeyValuePair<string, object?>(Resources.HttpRequestApiKeyOption, _apiKey));
#pragma warning restore CS0618

            // Act
            var response = _httpClient!.SendAsync(_fakeMessage!);

            // Assert
            Assert.IsNotNull(response);
        }
    }
}
