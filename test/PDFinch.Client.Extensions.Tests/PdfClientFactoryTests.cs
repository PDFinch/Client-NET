using System;
using NUnit.Framework;
using System.Net.Http;
using PDFinch.Client.Common;
using Moq;

namespace PDFinch.Client.Extensions.Tests
{
    public class PdfClientFactoryTests
    {
#pragma warning disable CS8618 // SetUp() sets it up, it's never null.
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        }

        [Test]
        public void GetPdfClient_Finds_SingleClient()
        {
            // Arrange
            var optionList = new[]
            {
                new PdfClientOptions
                {
                    Name = "name-01",
                    ApiKey = "key-01",
                    ApiSecret = "secret-01"
                }
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient(optionList[0].ApiKey))
                                  .Returns(new Mock<HttpClient>().Object);

            var classUnderTest = new PdfClientFactory(_httpClientFactoryMock.Object, new PdfClientOptionsList(optionList));

            // Act - these are the three ways users can look up clients, when a single (optionally named) one is registered.
            var namelessClient = classUnderTest.GetPdfClient();
            var namedClient = classUnderTest.GetPdfClient("name-01");
            var keyedClient = classUnderTest.GetPdfClient("key-01");

            // Assert
            Assert.NotNull(namelessClient);
            Assert.NotNull(namedClient);
            Assert.NotNull(keyedClient);
        }

        [Test]
        public void GetPdfClient_Calls_Factory_With_ApiKey()
        {
            // Arrange
            var optionList = new[]
            {
                new PdfClientOptions
                {
                    ApiKey = "key-01",
                    ApiSecret = "secret-01"
                }
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient(optionList[0].ApiKey))
                                  .Returns(new Mock<HttpClient>().Object);

            var classUnderTest = new PdfClientFactory(_httpClientFactoryMock.Object, new PdfClientOptionsList(optionList));

            // Act
            var client = classUnderTest.GetPdfClient(optionList[0].ApiKey);

            // Assert
            Assert.NotNull(client);
        }

        [Test]
        public void GetPdfClient_Finds_By_API_Key()
        {
            // Arrange
            var optionList = new[]
            {
                new PdfClientOptions
                {
                    ApiKey = "key-01",
                    ApiSecret = "secret-01"
                },
                new PdfClientOptions
                {
                    Name = "name-02",
                    ApiKey = "key-02",
                    ApiSecret = "secret-02"
                }
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient(optionList[0].ApiKey))
                                  .Returns(new Mock<HttpClient>().Object);

            _httpClientFactoryMock.Setup(f => f.CreateClient(optionList[1].ApiKey))
                                  .Returns(new Mock<HttpClient>().Object);

            var classUnderTest = new PdfClientFactory(_httpClientFactoryMock.Object, new PdfClientOptionsList(optionList));

            // Act & Assert

            // Unnamed doesn't work in combination with named.
            Assert.Throws<ArgumentNullException>(() => classUnderTest.GetPdfClient());

            // You can look up both on API key though.
            Assert.NotNull(classUnderTest.GetPdfClient(optionList[0].ApiKey));
            Assert.NotNull(classUnderTest.GetPdfClient(optionList[1].ApiKey));

            // And on name.
            Assert.NotNull(classUnderTest.GetPdfClient(optionList[1].Name));
        }

        [Test]
        public void PdfClientFactory_Throws_On_DuplicateName()
        {
            // Arrange
            var optionList = new[]
            {
                new PdfClientOptions { Name = "name-01", ApiKey = "client-01", ApiSecret = "secret-01" },
                new PdfClientOptions { Name = "name-01", ApiKey = "client-02", ApiSecret = "secret-02" },
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _ = new PdfClientFactory(_httpClientFactoryMock.Object, new PdfClientOptionsList(optionList)));
        }

        [Test]
        public void PdfClientFactory_Throws_On_DuplicateKey()
        {
            // Arrange
            var optionList = new[]
            {
                new PdfClientOptions { Name = "name-01", ApiKey = "client-01", ApiSecret = "secret-01" },
                new PdfClientOptions { Name = "name-02", ApiKey = "client-01", ApiSecret = "secret-02" },
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _ = new PdfClientFactory(_httpClientFactoryMock.Object, new PdfClientOptionsList(optionList)));
        }
    }
}
