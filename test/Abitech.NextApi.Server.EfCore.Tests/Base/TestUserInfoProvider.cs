using System.Threading.Tasks;
using Abitech.NextApi.Server.EfCore.DAL;

namespace Abitech.NextApi.Server.EfCore.Tests.Base
{
    public class TestUserInfoProvider: INextApiUserInfoProvider
    {
        public async Task<int?> CurrentUserId()
        {
            return 1;
        }
    }
}
