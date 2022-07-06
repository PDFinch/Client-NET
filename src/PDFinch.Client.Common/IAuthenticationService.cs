using System.Threading.Tasks;

namespace PDFinch.Client.Common
{
    /// <summary>
    /// Used to retrieve tokens a client can use to perform authenticated calls.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Using the registered client settings, tries to obtain an OAuth token for use in later API calls.
        ///
        /// Throws when that fails.
        /// </summary>
        Task<ResponseToken> GetTokenAsync(string apiKey);
    }
}
