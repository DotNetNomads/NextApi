using System.Threading.Tasks;
using Abitech.NextApi.Client;

namespace Abitech.NextApi.Server.Tests
{
    public class TestTokenProvider : INextApiAccessTokenProvider
    {
        private string _token;

        public TestTokenProvider(string token)
        {
            _token = token;
        }

#pragma warning disable 1998
        public async Task<string> ResolveToken()
#pragma warning restore 1998
        {
            return _token;
        }
    }
}
