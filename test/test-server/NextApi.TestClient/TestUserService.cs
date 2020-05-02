using NextApi.Client;
using NextApi.Common.Abstractions;
using NextApi.TestServer.DTO;

namespace NextApi.TestClient
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
