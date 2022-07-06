using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using PDFinch.Client.Common;
using Polly;

// ReSharper disable UnusedParameter.Local, ArgumentsStyleLiteral
namespace PDFinch.Client.Extensions
{
    /// <summary>
    /// Polly wants a cracker.
    /// </summary>
    internal static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Configures Polly for PdfClient.
        /// </summary>
        internal static IHttpClientBuilder ConfigureApiClientHandler(this IHttpClientBuilder builder, PdfClientOptions clientOptions)
        {
            return builder.SetHandlerLifetime(TimeSpan.FromMinutes(5))
                          .AddHttpMessageHandler<AuthenticationHandler>()
                          .AddPolicyHandler((provider, request) =>
                          {
                              return Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
                                  .RetryAsync(retryCount: 1, async (response, retryCount, context) =>
                                  {
                                      var client = provider.GetRequiredService<IAuthenticationService>();
                                      var responseToken = await client.GetTokenAsync(clientOptions.ApiKey);
                                      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", responseToken.AccessToken);
                                  });
                          })
                          .RetryOnce();
        }

        /// <summary>
        /// Configures Polly for authentication.
        /// </summary>
        internal static IHttpClientBuilder ConfigureAuthClientHandler(this IHttpClientBuilder builder)
        {
            return builder.SetHandlerLifetime(TimeSpan.FromMinutes(1))
                          .RetryOnce();
        }

        private static IHttpClientBuilder RetryOnce(this IHttpClientBuilder builder)
        {
            return builder.AddPolicyHandler((provider, request) =>
                           {
                               return Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.InternalServerError).RetryAsync(1);
                           });
        }
    }
}