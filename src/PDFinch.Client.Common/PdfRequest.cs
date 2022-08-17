namespace PDFinch.Client.Common
{
    /// <summary>
    /// Represents a request to generate a PDF from the given HTML and <see cref="PdfOptions"/>.
    /// </summary>
    public class PdfRequest : PdfOptions
    {
        /// <summary>
        /// The HTML to generate a PDF from.
        /// </summary>
        public string Html { get; }

        /// <summary>
        /// Instantiate a request with the given <paramref name="html"/>.
        /// </summary>
        public PdfRequest(string html)
        {
            Html = html;
        }
    }
}
