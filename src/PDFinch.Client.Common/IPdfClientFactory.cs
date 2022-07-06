namespace PDFinch.Client.Common
{
    /// <summary>
    /// Inject this interface into your class to create PDFinch API clients.
    /// </summary>
    public interface IPdfClientFactory
    {
        /// <summary>
        /// Retrieve an API client by name or id.
        /// </summary>
        /// <param name="nameOrApiKey">Required when multiple clients are registered.</param>
        IPdfClient GetPdfClient(string? nameOrApiKey = null);
    }
}
