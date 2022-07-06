using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PDFinch.Client.Common
{
    /// <summary>
    /// Implements base functionality of <see cref="IPdfClient"/> for an <see cref="HttpClient"/>.
    /// </summary>
    public abstract class HttpPdfClient : IPdfClient
    {
        /// <inheritdoc/>
        public string? Name { get; }

        /// <inheritdoc/>
        public string ApiKey { get; }

        /// <param name="pdfinchOptions"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected HttpPdfClient(PdfClientOptions pdfinchOptions)
        {
            Name = pdfinchOptions.Name;
            ApiKey = pdfinchOptions.ApiKey ?? throw new ArgumentNullException(nameof(pdfinchOptions.ApiKey));
        }

        /// <summary>
        /// Implement to return a client that will be authorized by the time it gets used to send the given message.
        /// </summary>
        protected abstract Task<HttpClient> AuthenticateClientAsync(HttpRequestMessage httpRequestMessage);

        /// <inheritdoc/>
        public async Task<PdfResult<Stream>> GeneratePdfFromHtmlAsync(string html, PdfOptions? options = null)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, Resources.CreatePdfEndpoint + options?.ToQueryString());
            
            httpRequestMessage.Content = new StringContent(html, Encoding.UTF8, "text/html");

            HttpClient? autenticatedHttpClient;

            try
            {
                autenticatedHttpClient = await AuthenticateClientAsync(httpRequestMessage);
            }
            catch (Exception ex)
            {
                return new PdfResult<Stream>(otherError: true, statusMessage: GetExceptionJson(ex));
            }

            // ReSharper disable once ConstantConditionalAccessQualifier - implementor can return null.
            if (autenticatedHttpClient?.BaseAddress == null)
            {
                const string errorMessage = $"{nameof(AuthenticateClientAsync)}() must return an {nameof(HttpClient)} with its {nameof(HttpClient.BaseAddress)} set";
                
                return new PdfResult<Stream>(otherError: true, statusMessage: PdfResult<Stream>.JsonStatus(errorMessage));
            }

            try
            {
                var response = await autenticatedHttpClient.SendAsync(httpRequestMessage);
                
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();

                    return new PdfResult<Stream>(stream);
                }
                if (response.StatusCode == HttpStatusCode.PaymentRequired)
                {
                    return PdfResult<Stream>.OutOfCredits(ApiKey);
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();

                    return new PdfResult<Stream>(otherError: true, statusMessage: json);
                }
            }
            catch (Exception ex)
            {
                return new PdfResult<Stream>(otherError: true, statusMessage: GetExceptionJson(ex));
            }
        }

        private static string GetExceptionJson(Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                Exception = ex.GetType().FullName,
                ex.Message,
                ex.StackTrace
            });
        }
    }
}
