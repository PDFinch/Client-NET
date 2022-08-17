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

        [Test]
        public async Task GeneratePdfFromHtmlAsync_PdfRequest_Uses_Parameters()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            };

            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var request = new PdfRequest("<h1>html</h1>")
            {
                Landscape = true,
                GrayScale = true,
                MarginLeft = 1,
                MarginRight = 2,
                MarginTop = 3,
                MarginBottom = 4,
            };

            handler.MockResponse((_, _) => new HttpResponseMessage(HttpStatusCode.OK));

            var client = new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("https://throw-when-called")
            };

            var classUnderTest = new Mock<HttpPdfClient>(MockBehavior.Strict, options);

            classUnderTest.Protected().Setup<Task<HttpClient>>("AuthenticateClientAsync", ItExpr.IsAny<HttpRequestMessage>())
                          .ReturnsAsync(client);

            // Act
            var pdfResult = await classUnderTest.Object.GeneratePdfFromHtmlAsync(request);

            // Assert
            Assert.IsTrue(pdfResult.Success);
            handler.VerifyAll();
        }

        [Test]
        public async Task GenerateMergedPdfFromHtmlAsync_Converts_Request_To_Multipart()
        {
            // Arrange
            var options = new PdfClientOptions
            {
                ApiKey = "key-01",
                ApiSecret = "secret-01"
            };

            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            
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

            handler.MockResponse((request, cancellationToken) =>
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

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var client = new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("https://throw-when-called")
            };

            var classUnderTest = new Mock<HttpPdfClient>(MockBehavior.Strict, options);

            classUnderTest.Protected().Setup<Task<HttpClient>>("AuthenticateClientAsync", ItExpr.IsAny<HttpRequestMessage>())
                          .ReturnsAsync(client);

            

            // Act
            var pdfResult = await classUnderTest.Object.GenerateMergedPdfFromHtmlAsync(requests);

            // Assert
            Assert.IsTrue(pdfResult.Success);
            handler.VerifyAll();
        }
    }
}