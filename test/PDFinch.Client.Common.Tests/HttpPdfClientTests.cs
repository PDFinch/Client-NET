using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Moq.Protected;
using PDFinch.Client.Tests.Shared;
using System.Threading;

namespace PDFinch.Client.Common.Tests
{
    public class HttpPdfClientTests
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _ok = (_, _) => new HttpResponseMessage(HttpStatusCode.OK);

#pragma warning disable CS8618 // SetUp
        private HttpClient _httpClient;
        private Mock<HttpMessageHandler> _httpHandlerMock;
        private HttpPdfClient _classUnderTest;
        private Mock<HttpPdfClient> _pdfClientMock;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            // Loose for non-overridable Dispose() that gets called.
            _httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            _httpClient = new HttpClient(_httpHandlerMock.Object)
            {
                BaseAddress = new Uri("https://throw-when-called")
            };
            
            var options = new PdfClientOptions { ApiKey = "api-01" };

            _pdfClientMock = new Mock<HttpPdfClient>(MockBehavior.Strict, options);

            _pdfClientMock.Protected().Setup<Task<HttpClient>>("AuthenticateClientAsync", ItExpr.IsAny<HttpRequestMessage>())
                          .ReturnsAsync(_httpClient)
                          .Verifiable();

            _classUnderTest = _pdfClientMock.Object;
        }

        private void SetUpRequests(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> authenticationResponse, Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> pdfResponse)
        {
            _httpHandlerMock.MockResponse((request, token) =>
                request.RequestUri?.AbsolutePath == "/" + Resources.OAuth2Endpoint
                    ? authenticationResponse(request, token) 
                    : pdfResponse(request, token))
                .Verifiable();
        }

        [Test]
        public async Task AuthenticateClientAsync_Handles_Exception()
        {
            // Arrange
            var options = new PdfClientOptions { ApiKey = "api-01" };

            var classUnderTest = new Mock<HttpPdfClient>(MockBehavior.Strict, options);

            classUnderTest.Protected().Setup<Task<HttpClient>>("AuthenticateClientAsync", ItExpr.IsAny<HttpRequestMessage>())
                          .ThrowsAsync(new InvalidOperationException("Tis but a test"))
                          .Verifiable();

            // Act
            var pdfResult = await classUnderTest.Object.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            // Assert
            Assert.IsFalse(pdfResult.Success);
            Assert.IsTrue(pdfResult.OtherError);
            Assert.IsFalse(pdfResult.IsOutOfCredits);

            classUnderTest.VerifyAll();
            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Requires_HttpClientBaseAddress()
        {
            // Arrange
            _httpClient.BaseAddress = null;

            // Act
            var pdfResult = await _classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            Assert.IsFalse(pdfResult.Success);
            Assert.IsTrue(pdfResult.StatusMessage!.Contains("BaseAddress"));

            _pdfClientMock.VerifyAll();
            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Handles_Exception()
        {
            // Arrange
            SetUpRequests(_ok, (_, _) => throw new InvalidOperationException("Tis but a test"));

            // Act
            var pdfResult = await _classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            Assert.IsFalse(pdfResult.Success);
            Assert.IsTrue(pdfResult.OtherError);
            Assert.IsFalse(pdfResult.IsOutOfCredits);

            _pdfClientMock.VerifyAll();
            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Calls_AuthenticateClientAsync()
        {
            // Arrange
            SetUpRequests(_ok, _ok);

            // Act
            var pdfResult = await _classUnderTest.GeneratePdfFromHtmlAsync("html");

            // Assert
            Assert.IsTrue(pdfResult.Success);

            _pdfClientMock.VerifyAll();
            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Handles_402()
        {
            // Arrange
            SetUpRequests(_ok, (_, _) => new HttpResponseMessage(HttpStatusCode.PaymentRequired));

            // Act
            var pdfResult = await _classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            Assert.IsFalse(pdfResult.Success);
            Assert.IsFalse(pdfResult.OtherError);
            Assert.IsTrue(pdfResult.IsOutOfCredits);

            _pdfClientMock.VerifyAll();
            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Handles_500()
        {
            // Arrange
            SetUpRequests(_ok, (_, _) => new HttpResponseMessage(HttpStatusCode.InternalServerError));

            // Act
            var pdfResult = await _classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            Assert.IsFalse(pdfResult.Success);
            Assert.IsTrue(pdfResult.OtherError);
            Assert.IsFalse(pdfResult.IsOutOfCredits);

            _pdfClientMock.VerifyAll();
            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_PdfRequest_Uses_Parameters()
        {
            // Arrange
            var pdfRequest = new PdfRequest("<h1>html</h1>")
            {
                Landscape = true,
                GrayScale = true,
                MarginLeft = 1,
                MarginRight = 2,
                MarginTop = 3,
                MarginBottom = 4,
            };
            
            SetUpRequests(_ok, (request, cancellationToken) =>
            {
                var query = request.RequestUri.ParseQueryString();

                Assert.AreEqual(pdfRequest.Html, request.Content!.ReadAsStringAsync(cancellationToken).Result);

                Assert.AreEqual(pdfRequest.Landscape.ToString(), query["landscape"]);
                Assert.AreEqual(pdfRequest.GrayScale.ToString(), query["grayscale"]);
                Assert.AreEqual(pdfRequest.MarginLeft.ToString(), query["marginleft"]);
                Assert.AreEqual(pdfRequest.MarginRight.ToString(), query["marginright"]);
                Assert.AreEqual(pdfRequest.MarginTop.ToString(), query["margintop"]);
                Assert.AreEqual(pdfRequest.MarginBottom.ToString(), query["marginbottom"]);

                return _ok(request, cancellationToken);
            });

            // Act
            var pdfResult = await _classUnderTest.GeneratePdfFromHtmlAsync(pdfRequest);

            // Assert
            Assert.IsTrue(pdfResult.Success);

            _pdfClientMock.VerifyAll();
            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GenerateMergedPdfFromHtmlAsync_Converts_Request_To_Multipart()
        {
            // Arrange
            var requests = new List<PdfRequest>
            {
                new ("<h1>html</h1>")
                {
                    Landscape = true,
                    GrayScale = true,
                    MarginLeft = 1,
                    MarginRight = 2,
                    MarginTop = 3,
                    MarginBottom = 4,
                },
                new ("<h2>html</h2>")
                {
                    Landscape = false,
                    GrayScale = false,
                    MarginLeft = 42,
                    MarginRight = 41,
                    MarginTop = 40,
                    MarginBottom = 39,
                }
            };

            SetUpRequests(_ok, (request, cancellationToken) =>
            {
                string? GetValue(MultipartStreamProvider provider, string name)
                {
                    var stringContent = provider.Contents.FirstOrDefault(c => c.Headers.ContentDisposition!.Name!.Equals($"\"{name}\"", StringComparison.InvariantCultureIgnoreCase));

                    return stringContent?.ReadAsStringAsync(cancellationToken).Result;
                }

                var requestBody = request.Content!.ReadAsMultipartAsync(cancellationToken).Result;

                Assert.AreEqual(requests[0].Html, GetValue(requestBody, "d[0].body"));
                Assert.AreEqual(requests[0].GrayScale.ToString(), GetValue(requestBody, "d[0].grayscale"));
                Assert.AreEqual(requests[0].Landscape.ToString(), GetValue(requestBody, "d[0].landscape"));
                Assert.AreEqual(requests[0].MarginLeft.ToString(), GetValue(requestBody, "d[0].marginleft"));
                Assert.AreEqual(requests[0].MarginRight.ToString(), GetValue(requestBody, "d[0].marginright"));
                Assert.AreEqual(requests[0].MarginTop.ToString(), GetValue(requestBody, "d[0].margintop"));
                Assert.AreEqual(requests[0].MarginBottom.ToString(), GetValue(requestBody, "d[0].marginbottom"));

                Assert.AreEqual(requests[1].Html, GetValue(requestBody, "d[1].body"));
                Assert.AreEqual(requests[1].GrayScale.ToString(), GetValue(requestBody, "d[1].grayscale"));
                Assert.AreEqual(requests[1].Landscape.ToString(), GetValue(requestBody, "d[1].landscape"));
                Assert.AreEqual(requests[1].MarginLeft.ToString(), GetValue(requestBody, "d[1].marginleft"));
                Assert.AreEqual(requests[1].MarginRight.ToString(), GetValue(requestBody, "d[1].marginright"));
                Assert.AreEqual(requests[1].MarginTop.ToString(), GetValue(requestBody, "d[1].margintop"));
                Assert.AreEqual(requests[1].MarginBottom.ToString(), GetValue(requestBody, "d[1].marginbottom"));

                return _ok(request, cancellationToken);
            });

            // Act
            var pdfResult = await _classUnderTest.GenerateMergedPdfFromHtmlAsync(requests);

            // Assert
            Assert.IsTrue(pdfResult.Success);

            _pdfClientMock.VerifyAll();
            _httpHandlerMock.VerifyAll();
        }
    }
}