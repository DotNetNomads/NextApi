using Abitech.NextApi.Client;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.TestServer.DTO;

namespace Abitech.NextApi.TestClient
{
    public interface ITestUserService : INextApiEntityService<TestUserDTO, int>
    {
    }

    public class TestUserService : NextApiEntityService<TestUserDTO, int, INextApiClient>, ITestUserService
    {
        public TestUserService(INextApiClient client) : base(client, "TestUser")
        {
        }
    }
}
