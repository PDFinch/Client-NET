namespace PDFinch.Client.Common
{
    /// <summary>
    /// PDFinch runs on multiple environments. You'd usually want to use Production.
    /// </summary>
    public enum PdfEnvironment
    {
        /// <summary>
        /// Staging environment.
        /// </summary>
        Staging = 0,

        /// <summary>
        /// Production environment.
        /// </summary>
        Production = 1,

        /// <summary>
        /// Custom environment, providing a base URL is mandatory.
        /// </summary>
        Custom = 2
    }
}
