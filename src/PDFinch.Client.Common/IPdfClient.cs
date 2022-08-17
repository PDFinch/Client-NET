using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PDFinch.Client.Common
{
    /// <summary>
    /// Used to call the PDFinch APIs.
    /// </summary>
    public interface IPdfClient
    {
        /// <summary>
        /// The configured name of this API client, if configured.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// The API key used to identify this client.
        /// </summary>
        string ApiKey { get; }

        /// <summary>
        /// Generate a PDF using the given HTML and options.
        /// </summary>
        Task<PdfResult<Stream>> GeneratePdfFromHtmlAsync(string html, PdfOptions? options = null);

        /// <summary>
        /// Generate a PDF using the given HTML and options.
        /// </summary>
        Task<PdfResult<Stream>> GeneratePdfFromHtmlAsync(PdfRequest pdfRequest);
        
        /// <summary>
        /// Generate a PDF using the given HTML and options.
        /// </summary>
        Task<PdfResult<Stream>> GenerateMergedPdfFromHtmlAsync(IEnumerable<PdfRequest> pdfRequests);
    }
}
