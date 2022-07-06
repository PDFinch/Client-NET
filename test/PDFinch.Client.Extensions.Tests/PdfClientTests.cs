using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PDFinch.Client.Common;
using PDFinch.Client.Tests.Shared;

namespace PDFinch.Client.Extensions.Tests
{
    public class PdfClientTests
    {
#pragma warning disable CS8618 // SetUp
        private HttpClient _httpClient;
        private Mock<HttpMessageHandler> _httpHandlerMock;
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
        }

        [Test]
        public async Task AuthenticateClientAsync_Sets_ApiKeyProperty()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "api-01",
                ApiSecret = "secret-01"
            };

            _httpHandlerMock.MockResponse((request, _) =>
            {
#pragma warning disable CS0618 // We know it's obsolete, but our core libraries target .NET Core 2, which doesn't have .Options. 
                Assert.IsTrue(request.Properties.ContainsKey("apikey"));
#pragma warning restore CS0618

                return new HttpResponseMessage(HttpStatusCode.OK);
            }).Verifiable();

            var classUnderTest = new PdfClient(_httpClient, options);

            // Act
            var pdfResult = await classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            Assert.IsTrue(pdfResult.Success);
            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Requires_HttpClientBaseAddress()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "api-01",
                ApiSecret = "secret-01"
            };

            _httpClient.BaseAddress = null;
            var classUnderTest = new PdfClient(_httpClient, options);

            // Act
            var pdfResult = await classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            Assert.IsFalse(pdfResult.Success);
            Assert.IsTrue(pdfResult.StatusMessage!.Contains("BaseAddress"));

            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Handles_402()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "api-01",
                ApiSecret = "secret-01"
            };

            _httpHandlerMock.MockResponse((request, _) => new HttpResponseMessage(HttpStatusCode.PaymentRequired)).Verifiable();

            var classUnderTest = new PdfClient(_httpClient, options);

            // Act
            var pdfResult = await classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            Assert.IsFalse(pdfResult.Success);
            Assert.IsFalse(pdfResult.OtherError);
            Assert.IsTrue(pdfResult.IsOutOfCredits);

            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Handles_500()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "api-01",
                ApiSecret = "secret-01"
            };

            _httpHandlerMock.MockResponse((request, _) => new HttpResponseMessage(HttpStatusCode.InternalServerError)).Verifiable();

            var classUnderTest = new PdfClient(_httpClient, options);

            // Act
            var pdfResult = await classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            Assert.IsFalse(pdfResult.Success);
            Assert.IsTrue(pdfResult.OtherError);
            Assert.IsFalse(pdfResult.IsOutOfCredits);

            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Handles_Exception()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "api-01",
                ApiSecret = "secret-01"
            };

            _httpHandlerMock.MockResponse((request, _) => throw new InvalidOperationException("Tis but a test")).Verifiable();

            var classUnderTest = new PdfClient(_httpClient, options);

            // Act
            var pdfResult = await classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            Assert.IsFalse(pdfResult.Success);
            Assert.IsTrue(pdfResult.OtherError);
            Assert.IsFalse(pdfResult.IsOutOfCredits);

            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public void NameFromOptions()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                Name = "Not Unused",
                ApiKey = "api-01",
                ApiSecret = "secret-01"
            };

            var classUnderTest = new PdfClient(_httpClient, options);

            Assert.AreEqual(options.Name, classUnderTest.Name);
        }
    }
}
