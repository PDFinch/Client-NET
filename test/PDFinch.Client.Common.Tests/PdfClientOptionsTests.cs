using System;
using NUnit.Framework;

namespace PDFinch.Client.Common.Tests
{
    internal class PdfClientOptionsTests
    {
        [Test]
        public void Unknown_Enviroment_Throws()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _ = new PdfClientOptions { Environment = (PdfEnvironment)4 }.GetBaseUrl());
        }

        [Test]
        public void GetBaseUrl_Uses_Environment()
        {
            // Act & Assert
            Assert.IsTrue(new PdfClientOptions { Environment = PdfEnvironment.Staging }.GetBaseUrl().Host.Contains("staging"));
        }

        [Test]
        public void GetBaseUrl_Without_BaseUrl_Throws()
        {
            // Arrange
            var classUnderTest = new PdfClientOptions
            {
                Environment = PdfEnvironment.Custom,
                BaseUrl = null
            };

            Assert.Throws<ArgumentNullException>(() => classUnderTest.GetBaseUrl());
        }

        [Test]
        public void GetBaseUrl_Reads_BaseUrl()
        {
            // Arrange
            var baseUrl = new Uri("https://www.example.com");
            var classUnderTest = new PdfClientOptions
            {
                Environment = PdfEnvironment.Custom,
                BaseUrl = baseUrl
            };

            Assert.AreEqual(baseUrl, classUnderTest.GetBaseUrl());
        }
    }
}
