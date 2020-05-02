using System.Threading.Tasks;

namespace NextApi.Client
{
    /// <summary>
    /// Provides access token
    /// </summary>
    public interface INextApiAccessTokenProvider
    {
        /// <summary>
        /// Resolves token for request
        /// </summary>
        /// <returns></returns>
        Task<string> ResolveToken();
    }
}
