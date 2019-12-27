using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Common.Abstractions.DAL;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.TestServer.DAL;
using Abitech.NextApi.TestServer.DTO;
using Abitech.NextApi.TestServer.Model;
using AutoMapper;

namespace Abitech.NextApi.TestServer.Service
{
    public class TestUserService : NextApiEntityService<TestUserDTO, TestUser, int>
    {
        public TestUserService(IUnitOfWork unitOfWork, IMapper mapper,
            IRepo<TestUser, int> repository) : base(
            unitOfWork, mapper, repository)
        {
        }
    }
}
