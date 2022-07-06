using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PDFinch.Client.Common;
using PDFinch.Client.Extensions;

namespace PDFinch.TestClient.Shared;

public static class DependencyInjectionSample
{
    public static async Task RunAsync()
    {
        // Your typical application startup.
        await Host.CreateDefaultBuilder().ConfigureServices((hostBuilder, services) =>
            {
                // Register your factory service, which uses the injected IPdfClientFactory.
                services.AddHostedService<SampleFactoryService>();

                // You would usually just need to call:
                //services.AddPDFinch(_configuration);

                // OR: using a custom config section name.
                services.AddPDFinch(hostBuilder.Configuration, configSectionName: "PDFinch-Dev")
                // ... and, optionally, manually add configs (here incomplete ones for demonstration purposes).
                        .AddPDFinch(new PdfClientOptions { ApiKey = "fail-01", ApiSecret = "secret-01" })
                        .AddPDFinch(new PDFinchClientSettings
                        {
                            Clients = new[]
                        {
                            new PdfClientOptions { ApiKey = "fail-02", ApiSecret = "secret-02" }
                        }
                        })
                        .AddPDFinch(options => { options.ApiKey = "fail-03"; options.ApiSecret = "secret-03"; });

                // OR: "PostConfigure" action, called _after_ loading settings from config.
                //services.AddPDFinch(hostBuilder.Configuration, /* optional: configSectionName: "YourCustomConfigSection", */ settings =>
                //{
                //    // Settings
                //    settings.Clients[0] = new PdfClientOptions
                //    {
                //        ApiKey = "your-api-key",
                //        ApiSecret = "your-api-secret",
                //    };
                //});

                // OR: Get your config manually from a configured configuration provider(s), usually appsettings.json.
                //var settingsObject = hostBuilder.Configuration
                //    .GetSection(PDFinchClientSettings.ConfigurationSectionName)
                //    .Get<PDFinchClientSettings>();
                //
                //services.AddPDFinch(settingsObject);

                // OR: manually configure and register a single client, can be called multiple times.
                services.AddPDFinch(new PdfClientOptions
                {
                    ApiKey = "your-api-key",
                    ApiSecret = "your-api-secret",
                    EnableCompression = true // This defaults to false
                });
            })
            .UseConsoleLifetime()
            .StartAsync();
    }

    public static async Task RunSingleClientAsync()
    {
        // Your typical application startup.
        await Host.CreateDefaultBuilder().ConfigureServices((hostBuilder, services) =>
            {
                // Register our factory service, which uses the injected IPdfClientFactory.
                services.AddHostedService<SampleFactoryService>();
                var settingsObject = hostBuilder.Configuration.GetRequiredSection("PDFinch-Dev").Get<PDFinchClientSettings>();

                // There must be at least one, single is true in the second run.
                services.AddPDFinch(settingsObject.Clients![0]);

                // Register our Client service, which uses the injected IPdfClient.
                services.AddHostedService<SampleClientService>();
            })
            .UseConsoleLifetime()
            .StartAsync();
    }
}