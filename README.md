# PDFinch Client for .NET
This repository contains the C# source code for the .NET clients to the PDFinch APIs. You should [get it on NuGet](https://www.nuget.org/packages/PDFinch.Client.Extensions/):

    PM> Install-Package PDFinch.Client.Extensions

You need to have an account with enough credits and an active set of API keys. You can [register an account or log in here](https://www.pdfinch.com/account/login).
# Basic usage
Our API currently supports one thing: generating PDFs from HTML, with some variations. The most simple way is available by calling `IPdfClient.GeneratePdfFromHtmlAsync()`:

```C#
IPdfClient pdfClient = ... // see chapter "Obtaining an IPdfClient" below

PdfResult<Stream> pdfResult = await pdfClient.GeneratePdfFromHtmlAsync("<h1>Your-Html-String</h1>", new PdfOptions
{
    MarginBottom = 10,
    MarginTop = 10,
    MarginLeft = 10,
    MarginRight = 10,
    Landscape = false,
    GrayScale = false,
});

if (pdfResult.Success)
{
    using var fileStream = File.Create("Temp/PDFinch-Output.pdf");
    await pdfResult.Data!.CopyToAsync(fileStream2);
}
else
{
    // TODO: handle, log pdfResult.StatusMessage
    throw new InvalidOperationException($"Error generating PDF: {pdfResult.StatusMessage}");
}
```

You can also merge multiple blocks of HTML:

```C#
PdfResult<Stream> pdfResult = await pdfClient.GenerateMergedPdfFromHtmlAsync(new []
{ 
    new PdfRequest("<h1>Your-Html-String</h1>")
    {
        MarginBottom = 10,
        MarginTop = 10,
        MarginLeft = 10,
        MarginRight = 10,
        Landscape = false,
        GrayScale = false,
    }, new PdfRequest("<h1>Your-Second-Html</h1>")
    {
        Landscape = true,
    },
});
```

# Handling responses
The `PDFinch.Client.Common.IPdfClient.Generate...Async()` methods return a `Task<PdfResult<Stream>>`. This means the call should be `await`ed, and the return value must be checked for success.

If `PdfResult<Stream>.Success` is `false`, `.Data` will be `null` and `.StatusMessage` will contain a machine-readable (JSON) error message returned by the API.

# Obtaining an IPdfClient
The easiest approach to create a client to issue the HTTP calls to the API uses Microsoft's dependency injection framework. This is the recommended approach, as it utilizes [Polly](https://github.com/App-vNext/Polly) for authentication and retries.

## Registration
You can configure the client using the configuration system in your `IHostBuilder.ConfigureServices((HostBuilderContext hostBuilder, IServiceCollection services) => { ... })` or anywhere during startup where you have an `IServiceCollection` and `IConfiguration` from Microsoft.Extensions.DependencyInjection and Microsoft.Extensions.Configuration:
```C#
// Default configuration section.
services.AddPDFinch(hostBuilder.Configuration);

// OR: Non-default configuration section name.
services.AddPDFinch(hostBuilder.Configuration, configSectionName: "PDFinch-Dev");

// OR: Get your config manually from a configured configuration provider(s), usually appsettings.json.
var settingsObject = hostBuilder.Configuration
    .GetSection(PDFinchClientSettings.ConfigurationSectionName)
    .Get<PDFinchClientSettings>();

services.AddPDFinch(settingsObject);
```

There's also the possibility of passing in client configuration objects, either as `PdfClientOptions` or as action:
```C#
services.AddPDFinch(new PdfClientOptions
{
    Name = "TestClient",
    ApiKey = "your-production-key-here",
    ApiSecret = "your-production-secret-here",
    Environment = PdfEnvironment.Production,
    EnableCompression = true
});

// OR
services.AddPDFinch(o => 
{
    o.Name = "TestClient";
    o.ApiKey = "your-production-key-here";
    o.ApiSecret = "your-production-secret-here";
    o.Environment = PdfEnvironment.Production;
    o.EnableCompression = true;
});
```

The `AddPDFinch()` methods can be called multiple times, in order to register multiple clients. Make sure the Name and ApiKey properties differs when you do so, otherwise an exception will be thrown.

### Configuration
When you call `AddPDFinch(IConfiguration[, ...])`, the configuration entry in your `appsettings.json` (or other configuration source) should look like this:
```json
{
  "PDFinch": {
    /* Either configure one client */
    "Name": "InvoiceClient", // Optional
    "ApiKey": "your-production-key-here",
    "ApiSecret": "your-production-secret-here",

    /* Or multiple clients */
    "Clients": [
      {
        "Name": "FirstClient", // Optional
        "ApiKey": "your-production-key-here",
        "ApiSecret": "your-production-secret-here"
        // Optional: "Environment": "[Production|Staging|Custom]", 
        // Optional: "BaseUrl": "https://your-api-url" when Environment: Custom
        // Optional: "EnableCompression": true
      },
      {
        "Name": "SecondClient", // Optional
        "ApiKey": "your-second-production-key-here",
        "ApiSecret": "your-production-secret-here"
        // Optional: "Environment": "[Production|Staging|Custom]", 
        // Optional: "BaseUrl": "https://your-api-url" when Environment: Custom
        // Optional: "EnableCompression": true
      }
    ]
  }
}
```

## Usage - IPdfClient
Only when you have registered one client, your services can take a dependency on `IPdfClient` directly. This client will be configured using the single configuration entry passed to the `AddPDFinch()` dependency injection configuration methods.

As soon as multiple client configurations are known to the DI framework, user code taking a dependency on `IPdfClient` will no longer be able to determine which client configuration to use (see #17), and throw an exception:

> InvalidOperationException: Unable to resolve service for type 'PDFinch.Client.Common.IPdfClient' while attempting to activate 'YourNamespace.YourService'.

In that case you'll need to inject an `IPdfClientFactory` to call `GetPdfClient(nameOrApiKey)` on.

## Usage - IPdfClientFactory
The `IPdfClientFactory` approach allows you to obtain clients where desired. This also works when you have registered a single client configuration.

If you configured the services as above using multiple client credentials, obtaining a client is done by taking a dependency on `IPdfClientFactory` and calling `GetPdfClient()`, passing either a name or API key (not the secret):

```C#
using PDFinch.Client.Common;

public class SampleUsage
{
    private readonly IPdfClientFactory _pdfClientFactory;

    public SampleService(IPdfClientFactory pdfClientFactory)
    {
        _pdfClientFactory = pdfClientFactory;
    }

    public async Task ExecuteAsync()
    {
        var pdfClient = _pdfClientFactory.GetPdfClient(/* "nameOrApiKey" */);

        var pdfResult = await pdfClient.GeneratePdf...

        if (pdfResult.Success)
        {
            // ...
        }
    }
}
```

## Registering and resolving multiple (named) clients
The configuration optionally exists of an array of clients, as seen under [Configuration](#configuration). You can register multiple clients:

```json
{
  "PDFinch": {
    "Clients": [
      {
        "Name": "InvoiceClient",
        "Environment": "Staging",
        "ApiKey": "your-invoiceclient-staging-key-here",
        "ApiSecret": "your-invoiceclient-staging-secret-here",
        "EnableCompression": true
      },
      {
        "Name": "ShippingLabelClient",
        "Environment": "Staging",
        "ApiKey": "your-shippinglabelclient-staging-key-here",
        "ApiSecret": "your-shippinglabelclient-staging-secret-here",
        "EnableCompression": true
      }
    ]
  }
}
```

When you have multiple clients registered, you _must_ pass the `nameOrApiKey` parameter to `IPdfClientFactory.GetPdfClient(string? nameOrApiKey)`:

```c#
public class ShippingLabelPrintingService
{
    private readonly IPdfClientFactory _pdfClientFactory;

    public ShippingLabelPrintingService(IPdfClientFactory pdfClientFactory)
    {
        _pdfClientFactory = pdfClientFactory;
    }

    public async Task ExecuteAsync()
    {
        var shippingLabelClient = _pdfClientFactory.GetPdfClient("ShippingLabelClient");

        var pdfResult = await shippingLabelClient.GeneratePdf...

        if (pdfResult.Success)
        {
            // ...
        }    
    }
}
```
Here, a name is used ("ShippingLabelClient"). Alternatively, you can request a client by passing its API key (not the secret). Note that names are optional, so if you register multiple clients, you must instruct your code for which API key it should reques a client.

Note that the configuration's name is merely a mnemonic to link configuration and usage together without having to remember (parts of) the API key. The name used in configuration is not related to the name entered while creating the API client credentials.

## Direct Usage
Recommended usage if you don't want to use dependency injection:
```C#
// Instantiate a factory
var factory = new PDFinch.Client.PdfClientFactory();

// Feed it with client credentials
factory.RegisterPdfClient(new PdfClientOptions
{
    ApiKey = "xxx",
    ApiSecret = "yyy"
});

// Obtain a client
var client = factory.GetPdfClient();

// Generate your PDF
var pdfResult = await pdfClient.GeneratePdf...
```

# Development
One-time setup: copy `appsettings.dist.json` to `appsettings.json` and properly populate its values.

## Solution Layout
* **PDFinch.Client.Extensions**: Client with dependency injection.
* **PDFinch.Client**: Client without dependency injection.
* **PDFinch.Client.Common**: Shared logic

# Publishing
The GitHub Action pipeline "Release-Applications.yml" will build and pack the library projects into NuGet packages and upload them to NuGet.

To generate packages to use during development, execute the PowerShell script `Publish-DevelopmentPackages.ps1` in the repository root. You can reference those if you add a filesystem NuGet provider pointing to the output directory.

# Contributing
Issues and PRs are welcome.
