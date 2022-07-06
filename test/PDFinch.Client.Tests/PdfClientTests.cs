using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PDFinch.Client.Common;
using PDFinch.Client.Tests.Shared;

#pragma warning disable CS8509 // Assert count, not in switch

namespace PDFinch.Client.Tests
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
        public async Task GeneratePdfFromHtmlAsync()
        {
            // Arrange
            var pdfData = "not-pdf";
            var options = new PdfClientOptions
            {
                ApiKey = "client-01",
                ApiSecret = "secret-01"
            };

            // First request is auth, 404 otherwise.
            _httpHandlerMock.SetupAuth((otherRequest, _) =>
                otherRequest.RequestUri!.ToString().Contains(Resources.CreatePdfEndpoint)
                    ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(pdfData) }
                    : new HttpResponseMessage(HttpStatusCode.NotFound
            ));

            var classUnderTest = new PdfClient(_httpClient, options);

            // Act
            var pdfResult = await classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");


            // Assert
            Assert.IsTrue(pdfResult.Success);
            if (pdfResult.Success)
            {
                var streamReader = new StreamReader(pdfResult.Data);
                Assert.AreEqual(pdfData, await streamReader.ReadToEndAsync());
            }

            _httpHandlerMock.VerifyAll();
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Caches_Token()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "client-01",
                ApiSecret = "secret-01"
            };

            int authCallCount = 0;
            int otherCallCount = 0;

            // Returns an empty PDF response.
            static HttpResponseMessage PdfResponse(HttpRequestMessage request) => 
                request.RequestUri!.ToString().Contains(Resources.CreatePdfEndpoint)
                        ? new HttpResponseMessage(HttpStatusCode.OK)
                        : new HttpResponseMessage(HttpStatusCode.NotFound);

            // Two requests to create a PDF.
            _httpHandlerMock.SetupAuth((otherRequest, _) => ++otherCallCount switch 
                {
                    // two PDFs
                    1 => PdfResponse(otherRequest),
                    2 => PdfResponse(otherRequest),
                }, () => authCallCount++);

            var classUnderTest = new PdfClient(_httpClient, options);

            // Act
            var pdfResult1 = await classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");
            var pdfResult2 = await classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            // Assert
            Assert.AreEqual(1, authCallCount);
            Assert.AreEqual(2, otherCallCount);
            
            Assert.IsTrue(pdfResult1.Success);
            Assert.IsTrue(pdfResult2.Success);
            
            _httpHandlerMock.VerifyAll();
        }
    }
}
