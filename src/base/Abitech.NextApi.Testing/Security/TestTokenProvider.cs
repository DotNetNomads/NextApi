using System.Threading.Tasks;
using Abitech.NextApi.Client;

namespace Abitech.NextApi.Server.Tests
{
    /// <summary>
    /// Test implementation of TokenProvider (with fake token provider)
    /// </summary>
    public class TestTokenProvider : INextApiAccessTokenProvider
    {
        private string _token;

        /// <inheritdoc />
        public TestTokenProvider(string token)
        {
            _token = token;
        }

#pragma warning disable 1998
        /// <inheritdoc />
        public async Task<string> ResolveToken()
#pragma warning restore 1998
        {
            return _token;
        }
    }
}
