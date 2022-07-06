using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PDFinch.Client.Common;

namespace PDFinch.Client.Extensions.Tests
{
    public class DependencyInjectionTests
    {
        private readonly PDFinchClientSettings _validClientSettings = new() { ApiKey = "client-01", ApiSecret = "secret-01" };
#pragma warning disable CS8618 // SetUp
        private ServiceCollection _serviceCollection;
#pragma warning restore CS8618

        [SetUp]
        public void SetUp()
        {
            // Arrange
            _serviceCollection = new ServiceCollection();

            // Clear clients between test runs.
            DependencyInjection.ClearConfigurationCache();
        }

        [Test]
        public void AddPDFinch_ParameterValidation()
        {
            // Arrange
            var singleClientAndClientList = new PDFinchClientSettings { ApiKey = "key-01", Clients = new[] { new PdfClientOptions { ApiKey = "key-02" } } };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddPDFinch((PdfClientOptions)_validClientSettings));
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddPDFinch(_validClientSettings));

            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddPDFinch(new PdfClientOptions()));
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddPDFinch((PdfClientOptions)null!));
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddPDFinch((PDFinchClientSettings)null!));
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddPDFinch((Action<PdfClientOptions>)null!));
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddPDFinch((IConfiguration)null!));

            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddPDFinch(o => { o.ApiKey = "no-secret"; }));
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddPDFinch(o => { o.ApiSecret = "no-key"; }));
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddPDFinch(new PdfClientOptions { ApiKey = "no-secret" }));
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddPDFinch(new PdfClientOptions { ApiSecret = "no-key" }));

            Assert.Throws<ArgumentException>(() => _serviceCollection.AddPDFinch(new PDFinchClientSettings()));
            Assert.Throws<ArgumentException>(() => _serviceCollection.AddPDFinch(singleClientAndClientList));
            Assert.Throws<ArgumentException>(() => _serviceCollection.AddPDFinch(new PDFinchClientSettings { Clients = null! }));
            Assert.Throws<ArgumentException>(() => _serviceCollection.AddPDFinch(new PDFinchClientSettings { Clients = new List<PdfClientOptions>() }));
        }

        [Test]
        public void AddPDFinch_Registration()
        {
            // Arrange
            var provider = _serviceCollection.AddPDFinch(options =>
            {
                options.ApiKey = _validClientSettings.ApiKey;
                options.ApiSecret = _validClientSettings.ApiSecret;
            }).BuildServiceProvider();

            // Act
            var pdfClientFactory = provider.GetRequiredService<IPdfClientFactory>();
            var pdfClient = pdfClientFactory.GetPdfClient();

            // Assert
            Assert.IsNotNull(pdfClient);
        }

        [Test]
        public void AddPDFinch_Requires_Configuration()
        {
            // Arrange
            var emptyConfigValue = new Mock<IConfigurationSection>(MockBehavior.Loose).Object;
            var configurationMock = new Mock<IConfiguration>(MockBehavior.Strict);

            configurationMock.Setup(c => c.GetSection(PDFinchClientSettings.ConfigurationSectionName))
                             .Returns(emptyConfigValue);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _serviceCollection.AddPDFinch(configurationMock.Object));
        }

        [Test]
        public void AddPDFinch_Validates_Configuration()
        {
            // Arrange
            var emptyConfigValueMock = new Mock<IConfigurationSection>(MockBehavior.Loose);
            emptyConfigValueMock.SetupGet(s => s.Path).Returns(PDFinchClientSettings.ConfigurationSectionName);

            var configurationMock = new Mock<IConfiguration>(MockBehavior.Strict);

            configurationMock.Setup(c => c.GetSection(PDFinchClientSettings.ConfigurationSectionName))
                             .Returns(emptyConfigValueMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _serviceCollection.AddPDFinch(configurationMock.Object));
        }

        [Test]
        public void AddPDFinch_Validates_NullConfiguration()
        {
            // Arrange
            var emptyConfigValueMock = new Mock<IConfigurationSection>(MockBehavior.Strict);
            emptyConfigValueMock.SetupGet(s => s.Value).Returns("");
            emptyConfigValueMock.SetupGet(s => s.Path).Returns(PDFinchClientSettings.ConfigurationSectionName);
            emptyConfigValueMock.SetupGet(s => s.Key).Returns(PDFinchClientSettings.ConfigurationSectionName);
            emptyConfigValueMock.Setup(s => s.GetChildren()).Returns(Array.Empty<IConfigurationSection>());

            var configurationMock = new Mock<IConfiguration>(MockBehavior.Strict);

            configurationMock.Setup(c => c.GetSection(PDFinchClientSettings.ConfigurationSectionName))
                             .Returns(emptyConfigValueMock.Object);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _serviceCollection.AddPDFinch(configurationMock.Object));
        }

        [Test]
        public void AddPDFinch_Reads_Configuration()
        {
            // Arrange
            var emptyConfigValue = new Mock<IConfigurationSection>(MockBehavior.Loose).Object;
            var configurationMock = new Mock<IConfiguration>(MockBehavior.Strict);
            var configurationSectionMock = new Mock<IConfigurationSection>(MockBehavior.Strict);

            configurationMock.Setup(c => c.GetSection(PDFinchClientSettings.ConfigurationSectionName))
                             .Returns(configurationSectionMock.Object);

            configurationSectionMock.SetupGet(s => s.Value)
                                    .Returns((string)null!);

            configurationSectionMock.SetupGet(s => s.Path)
                                    .Returns(PDFinchClientSettings.ConfigurationSectionName);

            var apiKeySection = new Mock<IConfigurationSection>(MockBehavior.Strict);
            apiKeySection.SetupGet(s => s.Value).Returns("key-01");
            apiKeySection.SetupGet(s => s.Path).Returns($"{PDFinchClientSettings.ConfigurationSectionName}:ApiKey");
            apiKeySection.Setup(s => s.GetChildren()).Returns(Array.Empty<IConfigurationSection>());

            var apiSecretSection = new Mock<IConfigurationSection>(MockBehavior.Strict);
            apiSecretSection.SetupGet(s => s.Value).Returns("secret-01");
            apiSecretSection.SetupGet(s => s.Path).Returns($"{PDFinchClientSettings.ConfigurationSectionName}:Secret");
            apiSecretSection.Setup(s => s.GetChildren()).Returns(Array.Empty<IConfigurationSection>());

            configurationSectionMock.Setup(s => s.GetSection("Clients")).Returns(emptyConfigValue);
            configurationSectionMock.Setup(s => s.GetSection("ApiKey")).Returns(apiKeySection.Object);
            configurationSectionMock.Setup(s => s.GetSection("ApiSecret")).Returns(apiSecretSection.Object);
            configurationSectionMock.Setup(s => s.GetSection("Name")).Returns(emptyConfigValue);
            configurationSectionMock.Setup(s => s.GetSection("Environment")).Returns(emptyConfigValue);
            configurationSectionMock.Setup(s => s.GetSection("BaseUrl")).Returns(emptyConfigValue);
            configurationSectionMock.Setup(s => s.GetSection("EnableCompression")).Returns(emptyConfigValue);

            configurationSectionMock.Setup(s => s.GetChildren())
                                    .Returns(new[]
                                    {
                                        apiKeySection.Object,
                                        apiSecretSection.Object,
                                    });

            // Act
            var provider = _serviceCollection.AddPDFinch(configurationMock.Object).BuildServiceProvider();
            var pdfClient = provider.GetService<IPdfClient>();

            // Assert
            Assert.IsNotNull(pdfClient);
        }

        [Test]
        public void AddPDFinch_Configures_IAuthenticationService()
        {
            // Arrange
            var provider = _serviceCollection.AddPDFinch(_validClientSettings).BuildServiceProvider();

            // Act
            var authenticationService = provider.GetService<IAuthenticationService>();

            // Assert
            Assert.NotNull(authenticationService);
        }

        [Test]
        public void AddPDFinch_Configures_IPdfClient()
        {
            // Arrange
            var provider = _serviceCollection.AddPDFinch(_validClientSettings).BuildServiceProvider();

            // Act
            var pdfClient = provider.GetService<IPdfClient>();

            // Assert
            Assert.NotNull(pdfClient);
        }

        [Test]
        public void AddPDFinch_Clients_Configures_IPdfClientFactory()
        {
            // Arrange
            var settings = new PDFinchClientSettings
            {
                Clients = new List<PdfClientOptions>
                {
                    new() { ApiKey = "client-01", ApiSecret = "secret-01" },
                    new() { ApiKey = "client-02", ApiSecret = "secret-02" },
                }
            };

            // Act
            var provider = _serviceCollection.AddPDFinch(settings)
                                             .BuildServiceProvider();

            var pdfClient = provider.GetService<IPdfClientFactory>();

            // Assert
            Assert.NotNull(pdfClient);
        }

        [Test]
        public void AddPDFinch_Twice_Configures_IPdfClientFactory()
        {
            // Arrange
            var clientSettings1 = new PDFinchClientSettings { ApiKey = "client-01", ApiSecret = "secret-01" };
            var clientSettings2 = new PDFinchClientSettings { ApiKey = "client-02", ApiSecret = "secret-02" };

            // Act
            var provider = _serviceCollection.AddPDFinch(clientSettings1)
                                             .AddPDFinch(clientSettings2)
                                             .BuildServiceProvider();

            var pdfClient = provider.GetService<IPdfClientFactory>();

            // Assert
            Assert.NotNull(pdfClient);
        }

        [Test]
        public void AddPDFinch_Configures_AuthClient()
        {
            // Arrange
            var clientSettings1 = new PDFinchClientSettings { ApiKey = "client-01", ApiSecret = "secret-01" };
            var clientSettings2 = new PDFinchClientSettings { ApiKey = "client-02", ApiSecret = "secret-02" };

            // Act
            var provider = _serviceCollection.AddPDFinch(clientSettings1)
                                             .AddPDFinch(clientSettings2)
                                             .BuildServiceProvider();

            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

            // Do not use this client, it issues real requests.
            var authHttpClient = httpClientFactory.CreateClient(Resources.AuthenticationClientID);

            // Assert
            Assert.NotNull(authHttpClient);
        }
    }
}
