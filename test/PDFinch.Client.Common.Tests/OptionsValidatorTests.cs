using System;
using System.Linq;
using NUnit.Framework;
using PDFinch.Client.Common.Internal;

namespace PDFinch.Client.Common.Tests
{
    public class OptionsValidatorTests
    {
        [Test]
        public void RegisterPdfClient_KeyAndNameUnique()
        {
            // Arrange
            var options = new[]
            {
                new PdfClientOptions { ApiKey = "client-01", ApiSecret = "secret-01" },
                new PdfClientOptions { Name = "name-01", ApiKey = "client-02", ApiSecret = "secret-02" }
            };

            // Act & Assert
            var keyAlreadyRegistered = options.Concat(new[] { new PdfClientOptions { ApiKey = "client-01", ApiSecret = "secret-99" } });
            Assert.Throws<ArgumentException>(() => OptionsValidator.ValidateOptions(keyAlreadyRegistered));

            var nameAlreadyRegistered = options.Concat(new [] { new PdfClientOptions { Name = "name-01", ApiKey = "client-03", ApiSecret = "secret-99" } });
            Assert.Throws<ArgumentException>(() => OptionsValidator.ValidateOptions(nameAlreadyRegistered));

            Assert.Throws<ArgumentException>(() => OptionsValidator.ValidateOptions(Array.Empty<PdfClientOptions>()));
        }
    }
}
