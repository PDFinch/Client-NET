using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PDFinch.Client.Common;

#pragma warning disable CS0419, CS1574 // Ambiguous reference in cref attribute - applies to all overloads.
// ReSharper disable InconsistentNaming - We're called "PDFinch".
namespace PDFinch.Client.Extensions
{
    /// <summary>
    /// Extension methods to add and configure the PDFinch API client using Dependency Injection.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Remembers all options during DI.
        /// </summary>
        private static readonly List<PdfClientOptions> _allClientOptions = new();

        /// <summary>
        /// Does the actual registration for the given configuration(s), called by all <see cref="AddPDFinch"/> overloads.
        /// </summary>
        private static IServiceCollection AddPDFinch(this IServiceCollection services, IEnumerable<PdfClientOptions> clientOptions)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            _allClientOptions.AddRange(clientOptions);

            // Throws when the (new) configuration is invalid.
            var validatedOptions = new PdfClientOptionsList(_allClientOptions);

            // Clears earlier registrations.
            RegisterRequiredServices(services, validatedOptions);

            // TODO: later make public and return the builder itself.
            var builder = new PDFinchBuilder(services, validatedOptions)
                .AddPdfClientFactory();
            
            // Injecting an IPdfClient is currently only supported if the user has exactly one client configured.
            if (validatedOptions.GetAll().Count == 1)
            {
                builder.AddPdfClient();
            }

            return builder.Services;
        }

        /// <summary>
        /// Register a single PDFinch API client for <see cref="IPdfClientFactory"/> dependency injection.
        /// </summary>
        public static IServiceCollection AddPDFinch(this IServiceCollection services, PdfClientOptions clientOptions)
        {
            if (clientOptions == null) throw new ArgumentNullException(nameof(clientOptions));

            return services.AddPDFinch(new[] { clientOptions });
        }

        /// <summary>
        /// Register a single PDFinch API client for <see cref="IPdfClientFactory"/> dependency injection using a configuration action.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction">Used to configure the <see cref="PdfClientOptions"/> (not compatible with <see cref="Microsoft.Extensions.Options.IOptions{PdfClientOptions}"/>).</param>
        public static IServiceCollection AddPDFinch(this IServiceCollection services, Action<PdfClientOptions> setupAction)
        {
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            var options = new PdfClientOptions();

            setupAction(options);

            return services.AddPDFinch(options);
        }

        /// <summary>
        /// Register PDFinch API clients through <see cref="IPdfClientFactory"/> dependency injection by reading the relevant section from the configuration system. 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="setupAction">Used to configure the <see cref="PdfClientOptions"/> (not compatible with <see cref="Microsoft.Extensions.Options.IOptions{PdfClientOptions}"/>).</param>
        /// <param name="configSectionName">Defaults to <see cref="PDFinchClientSettings.ConfigurationSectionName"/>.</param>
        public static IServiceCollection AddPDFinch(this IServiceCollection services, IConfiguration configuration, Action<PDFinchClientSettings>? setupAction = null, string? configSectionName = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            configSectionName ??= PDFinchClientSettings.ConfigurationSectionName;

            var section = configuration.GetRequiredSection(configSectionName);

            var settingsObject = section.Get<PDFinchClientSettings>();

            if (settingsObject == null)
            {
                throw new ArgumentException($"Configuration section '{configSectionName}' not deserializable to {nameof(PDFinchClientSettings)}", nameof(configSectionName));
            }

            // "PostConfigure". We cannot support IOptions<>'s PostConfigure, because we need it _now_ (settingsObject.Clients below). 
            // TODO: refactor to support IOptions<>'s PostConfigure?
            setupAction?.Invoke(settingsObject);

            // Maybe passes null or empty, next method will throw.
            return services.AddPDFinch(settingsObject);
        }

        /// <summary>
        /// Register PDFinch API clients for <see cref="IPdfClientFactory"/> dependency injection from a configuration object.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="clientOptions">A settings object that you obtained or created.</param>
        public static IServiceCollection AddPDFinch(this IServiceCollection services, PDFinchClientSettings clientOptions)
        {
            if (clientOptions == null) throw new ArgumentNullException(nameof(clientOptions));

            if (clientOptions.ApiKey != null && clientOptions.Clients?.Any() == true)
            {
                throw new ArgumentException("Configure either a single client or an array, not both", nameof(clientOptions));
            }
            // ReSharper disable once ConstantConditionalAccessQualifier - public method, callers can pass null.
            if (clientOptions.ApiKey == null && clientOptions?.Clients?.Any() != true)
            {
                throw new ArgumentException("Configure either a single client or an array, not neither", nameof(clientOptions));
            }

            // No client-collection? Pass the config object itself. Because of inheritance.
            if (!clientOptions.Clients!.Any())
            {
                return services.AddPDFinch((PdfClientOptions)clientOptions);
            }

            return services.AddPDFinch(clientOptions.Clients!);
        }

        private static void RegisterRequiredServices(IServiceCollection services, PdfClientOptionsList optionsList)
        {
            // Only register once.
            if (services.Any(s => s.ServiceType == typeof(IPdfClientFactory)))
            {
                // We are called for a second time. Clean up the previous run, as the current AddPDFinch() will
                // append to the registered clients and register all clients and the factory again.
                services.RemoveAll(typeof(IPdfClient));
                services.RemoveAll(typeof(IPdfClientFactory));

                // TODO: when being overwritten, the previous registrations of AddHttpClient
                // and ConfigureApiClientHandler for each client remain in the DI I'm afraid?

                return;
            }

            services.AddScoped<IAuthenticationService>(s => new AuthenticationService(s.GetRequiredService<IHttpClientFactory>(), optionsList));

            services.AddTransient<AuthenticationHandler>();

            // Add a client specifically for authentication, we don't want to have the PDF API client's handler configuration.
            services.AddHttpClient(Resources.AuthenticationClientID).ConfigureAuthClientHandler();
        }

        /// <summary>
        /// Called when a <see cref="PdfClient"/> is instantiated, to invalidate the configuration cache.
        /// </summary>
        public static void ClearConfigurationCache()
        {
            // We don't know when user code is done, so when is it safe to clear this bag of credentials out of memory?
            // Currently as soon as someone requests a client, clear the configuration cache. See PdfClient.
            // The configuration is not needed anymore, each client holds on to their own credentials.
            
            _allClientOptions.Clear();
        }
    }
}
