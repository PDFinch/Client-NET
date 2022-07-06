using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PDFinch.Client;
using PDFinch.Client.Common;

namespace PDFinch.TestClient.Shared
{
    public static class DirectUsageSample
    {
        public static async Task RunAsync()
        {
            // No dependency injection.
            IPdfClientFactory factory = await GetConfiguredFactory();

            IPdfClient client = factory.GetPdfClient("Staging");

            PdfResult<Stream> pdfResult = await client.GeneratePdfFromHtmlAsync($"<h1>Staging</h1><p>Generated on {DateTime.Now:F}</p>");

            if (pdfResult.Success)
            {
                using var fileStream = File.Create($"Temp/PDFinch-Staging-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.pdf");

                await pdfResult.Data!.CopyToAsync(fileStream);
            }
            else
            {
                // TODO: handle, log orderPdfResult.StatusMessage
                throw new InvalidOperationException($"Error generating PDF: {pdfResult.StatusMessage}");
            }
        }

        private static async Task<IPdfClientFactory> GetConfiguredFactory()
        {
            var factory = new PdfClientFactory();

            PDFinchClientSettings settings = await ReadSettingsAsync();

            foreach (var clientOptions in settings.Clients!)
            {
                factory.RegisterPdfClient(clientOptions);
            }

            return factory;
        }

        private static async Task<PDFinchClientSettings> ReadSettingsAsync()
        {
            // No config system in this part of the sample, but if you're in your own ConfigureServices(),
            // you should use something like hostBuilder.Configuration.GetSection(...)
            // as shown in the DependencyInjectionSample class.
            var appSettings = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonDocument>>(File.OpenRead("appsettings.json"));

            PDFinchClientSettings? settings = null;

            if (appSettings?.TryGetValue(PDFinchClientSettings.ConfigurationSectionName + "-Dev", out var settingsJson) == true)
            {
                var options = new JsonSerializerOptions();

                options.Converters.Add(new JsonStringEnumConverter());

                settings = settingsJson?.Deserialize<PDFinchClientSettings>(options);
            }
            
            if (settings?.Clients?.Any() != true)
            {
                throw new InvalidOperationException($"Configuration section '{PDFinchClientSettings.ConfigurationSectionName}' invalid or empty.");
            }

            return settings;
        }
    }
}
