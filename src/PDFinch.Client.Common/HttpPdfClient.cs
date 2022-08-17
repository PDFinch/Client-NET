using System;
using System.Collections.Generic;
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
        public Task<PdfResult<Stream>> GeneratePdfFromHtmlAsync(PdfRequest pdfRequest) => GeneratePdfFromHtmlAsync(pdfRequest.Html, pdfRequest);

        /// <inheritdoc/>
        public async Task<PdfResult<Stream>> GeneratePdfFromHtmlAsync(string html, PdfOptions? options = null)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, Resources.CreatePdfEndpoint + options?.ToQueryString())
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            };

            return await ExecuteRequestAsync(httpRequestMessage);
        }

        /// <inheritdoc/>
        public async Task<PdfResult<Stream>> GenerateMergedPdfFromHtmlAsync(IEnumerable<PdfRequest> pdfRequests)
        {
            var formContent = new MultipartFormDataContent();

            var i = 0;

            foreach (var request in pdfRequests)
            {
                formContent.Add(new StringContent(request.Html, Encoding.UTF8, "text/html"), $"d[{i}].body");
                
                formContent.Add(new StringContent(request.Landscape.ToString()), $"d[{i}].landscape");
                formContent.Add(new StringContent(request.GrayScale.ToString()), $"d[{i}].grayscale");
                formContent.Add(new StringContent(request.MarginLeft.ToString()), $"d[{i}].marginleft");
                formContent.Add(new StringContent(request.MarginRight.ToString()), $"d[{i}].marginright");
                formContent.Add(new StringContent(request.MarginTop.ToString()), $"d[{i}].margintop");
                formContent.Add(new StringContent(request.MarginBottom.ToString()), $"d[{i}].marginbottom");

                i++;
            }

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, Resources.MergePdfEndpoint)
            {
                Content = formContent
            };

            return await ExecuteRequestAsync(httpRequestMessage);
        }

        private async Task<PdfResult<Stream>> ExecuteRequestAsync(HttpRequestMessage httpRequestMessage)
        {
            try
            {
                var autenticatedHttpClient = await AuthenticateClientImplAsync(httpRequestMessage);

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

        private async Task<HttpClient> AuthenticateClientImplAsync(HttpRequestMessage httpRequestMessage)
        {
            var autenticatedHttpClient = await AuthenticateClientAsync(httpRequestMessage);

            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - implementor can return null.
            if (autenticatedHttpClient?.BaseAddress == null)
            {
                throw new InvalidOperationException($"{nameof(AuthenticateClientAsync)}() must return an {nameof(HttpClient)} with its {nameof(HttpClient.BaseAddress)} set");
            }

            return autenticatedHttpClient;
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
