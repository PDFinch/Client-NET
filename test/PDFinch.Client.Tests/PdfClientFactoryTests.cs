using System;
using NUnit.Framework;
using PDFinch.Client.Common;

namespace PDFinch.Client.Tests
{
    public class PdfClientFactoryTests
    {
        private PdfClientFactory _classUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new PdfClientFactory();
        }

        [Test]
        public void RegisterPdfClient_ParameterValidation()
        {
            // Missing key
            Assert.Throws<ArgumentNullException>(() => _classUnderTest.RegisterPdfClient(new PdfClientOptions { ApiSecret = "secret-01" }));

            // Missing secret
            Assert.Throws<ArgumentNullException>(() => _classUnderTest.RegisterPdfClient(new PdfClientOptions { ApiKey = "client-01" }));

            // Missing key and secret
            Assert.Throws<ArgumentNullException>(() => _classUnderTest.RegisterPdfClient(new PdfClientOptions { Name = "name-01" }));

            // Missing everything
            Assert.Throws<ArgumentNullException>(() => _classUnderTest.RegisterPdfClient(new PdfClientOptions()));
        }

        [Test]
        public void GetPdfClient_ParameterValidation()
        {
            // Missing key
            Assert.Throws<InvalidOperationException>(() => _classUnderTest.GetPdfClient("client-01"));
        }

        [Test]
        public void RegisterPdfClient_One_DefaultClient()
        {
            // Arrange
            _classUnderTest.RegisterPdfClient(new PdfClientOptions { ApiKey = "client-01", ApiSecret = "secret-01" });

            // Act & Assert
            Assert.IsNotNull(_classUnderTest.GetPdfClient());
            Assert.IsNotNull(_classUnderTest.GetPdfClient("client-01"));
            Assert.Throws<ArgumentException>(() => _classUnderTest.GetPdfClient(""));
            Assert.Throws<ArgumentException>(() => _classUnderTest.GetPdfClient("DefaultClient"));
        }

        [Test]
        public void RegisterPdfClient_Multiple_DefaultClient()
        {
            // Arrange
            _classUnderTest.RegisterPdfClient(new PdfClientOptions { ApiKey = "client-01", ApiSecret = "secret-01" });
            _classUnderTest.RegisterPdfClient(new PdfClientOptions { Name = "name-01", ApiKey = "client-02", ApiSecret = "secret-02" });

            // Act & Assert
            Assert.IsNotNull(_classUnderTest.GetPdfClient("name-01"));
            Assert.IsNotNull(_classUnderTest.GetPdfClient("client-01"));

            Assert.Throws<ArgumentException>(() => _classUnderTest.GetPdfClient(""));
            Assert.Throws<ArgumentException>(() => _classUnderTest.GetPdfClient("DefaultClient"));
        }

        [Test]
        public void RegisterPdfClient_Multiple_NamedDefaultClient()
        {
            // Arrange
            _classUnderTest.RegisterPdfClient(new PdfClientOptions { ApiKey = "client-01", ApiSecret = "secret-01" });
            _classUnderTest.RegisterPdfClient(new PdfClientOptions { Name = "name-01", ApiKey = "client-02", ApiSecret = "secret-02" });

            // Act & Assert
            Assert.IsNotNull(_classUnderTest.GetPdfClient("name-01"));
            Assert.IsNotNull(_classUnderTest.GetPdfClient("client-01"));

            Assert.Throws<ArgumentException>(() => _classUnderTest.GetPdfClient(""));
            Assert.Throws<ArgumentException>(() => _classUnderTest.GetPdfClient("DefaultClient"));
        }
    }
}
