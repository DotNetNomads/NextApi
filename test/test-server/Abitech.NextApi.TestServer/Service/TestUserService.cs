using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.TestServer.DAL;
using Abitech.NextApi.TestServer.DTO;
using Abitech.NextApi.TestServer.Model;
using AutoMapper;

namespace Abitech.NextApi.TestServer.Service
{
    public class TestUserService : NextApiEntityService<TestUserDTO, TestUser, int, TestUserRepository, TestUnitOfWork>
    {
        public TestUserService(TestUnitOfWork unitOfWork, IMapper mapper, TestUserRepository repository) : base(
            unitOfWork, mapper, repository)
        {
        }
    }
}
