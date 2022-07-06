using System;
using System.Net;
using System.Net.Http;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Moq.Protected;
using PDFinch.Client.Tests.Shared;

namespace PDFinch.Client.Common.Tests
{
    public class HttpPdfClientTests
    {
        [Test]
        public async Task AuthenticateClientAsync_Handles_Exception()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                Name = "Throws",
                ApiKey = "api-01",
                ApiSecret = "secret-01"
            };

            var classUnderTest = new BadPdfClient(options);

            // Act
            var pdfResult = await classUnderTest.GeneratePdfFromHtmlAsync("<h1>Html</h1>");

            // Assert
            Assert.IsFalse(pdfResult.Success);
            Assert.IsTrue(pdfResult.OtherError);
            Assert.IsFalse(pdfResult.IsOutOfCredits);
        }

        [Test]
        public async Task GeneratePdfFromHtmlAsync_Calls_AuthenticateClientAsync()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            };

            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handler.MockResponse((_, _) => new HttpResponseMessage(HttpStatusCode.OK));

            var client = new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("https://throw-when-called")
            };

            var classUnderTest = new Mock<HttpPdfClient>(MockBehavior.Strict, options);
            
            classUnderTest.Protected().Setup<Task<HttpClient>>("AuthenticateClientAsync", ItExpr.IsAny<HttpRequestMessage>())
                          .ReturnsAsync(client);

            // Act
            var pdfResult = await classUnderTest.Object.GeneratePdfFromHtmlAsync("html");

            // Assert
            Assert.IsTrue(pdfResult.Success);
            handler.VerifyAll();
        }
    }
}