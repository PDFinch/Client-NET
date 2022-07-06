using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PDFinch.Client.Common;

namespace PDFinch.Client.Extensions.Tests
{
    // ReSharper disable once InconsistentNaming - We're called "PDFinch".
    public class PDFinchBuilderTests
    {
        [Test]
        public void AddPdfClient_Sets_BaseAddressAndUserAgent()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            var authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);

            services.AddScoped(s => new Mock<AuthenticationHandler>(MockBehavior.Strict, authenticationServiceMock.Object).Object);

            var options = new PdfClientOptions
            {
                Name = "name-01",
                ApiKey = "client-01",
                ApiSecret = "secret-01"
            };

            var optionList = new[] { options };

            var classUnderTest = new PDFinchBuilder(services, new PdfClientOptionsList(optionList));

            // Act
            classUnderTest.AddPdfClient(options.ApiKey);

            var container = services.BuildServiceProvider();

            var httpClientFactory = container.GetRequiredService<IHttpClientFactory>();

            var httpClient = httpClientFactory.CreateClient(options.ApiKey);

            // Assert
            Assert.IsNotNull(httpClient);
            Assert.IsNotNull(httpClient.BaseAddress);

            var userAgent = httpClient.DefaultRequestHeaders.UserAgent.FirstOrDefault()?.ToString();
            Assert.IsTrue(userAgent?.Contains("PDFinch"));
        }

        [Test]
        public void AddPdfClient_Adds_EncodingHeaders()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            var authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);

            services.AddScoped(s => new Mock<AuthenticationHandler>(MockBehavior.Strict, authenticationServiceMock.Object).Object);

            var options = new PdfClientOptions
            {
                Name = "name-01",
                ApiKey = "client-01",
                ApiSecret = "secret-01",
                EnableCompression = true
            };

            var optionList = new[] { options };

            var classUnderTest = new PDFinchBuilder(services, new PdfClientOptionsList(optionList));

            // Act
            classUnderTest.AddPdfClient(options.ApiKey);

            var container = services.BuildServiceProvider();

            var httpClientFactory = container.GetRequiredService<IHttpClientFactory>();

            var httpClient = httpClientFactory.CreateClient(options.ApiKey);

            // Assert
            Assert.IsNotNull(httpClient);
            Assert.IsNotNull(httpClient.BaseAddress);

            Assert.IsTrue(httpClient.DefaultRequestHeaders.AcceptEncoding.Any(x => x.Value is "gzip" or "deflate"));
        }
    }
}
