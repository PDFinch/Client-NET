namespace PDFinch.Client.Common
{
    /// <summary>
    /// Contains the options that can be configured when generating a PDF document.
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class PdfOptions
    {
        /// <summary>
        /// Left margin in pixels.
        /// </summary>
        public int MarginLeft { get; set; }
        
        /// <summary>
        /// Right margin in pixels.
        /// </summary>
        public int MarginRight { get; set; }

        /// <summary>
        /// Top margin in pixels.
        /// </summary>
        public int MarginTop { get; set; }

        /// <summary>
        /// Bottom margin in pixels.
        /// </summary>
        public int MarginBottom { get; set; }

        /// <summary>
        /// Whether to render the document in gray scale.
        /// </summary>
        public bool GrayScale { get; set; }

        /// <summary>
        /// Whether to render the document in landscape. The default, <c>false</c>, is in portrait.
        /// </summary>
        public bool Landscape { get; set; }

        /// <summary>
        /// Appends all known properties into a query string.
        /// </summary>
        /// <returns></returns>
        public virtual string ToQueryString()
        {
            return "?MarginLeft=" + MarginLeft
                + "&MarginRight=" + MarginRight
                + "&MarginTop=" + MarginTop
                + "&MarginBottom=" + MarginBottom
                + "&GrayScale=" + GrayScale
                + "&Landscape=" + Landscape;
        }
    }
}
