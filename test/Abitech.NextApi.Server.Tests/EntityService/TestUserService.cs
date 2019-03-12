using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.Tests.EntityService.DTO;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using AutoMapper;

namespace Abitech.NextApi.Server.Tests.EntityService
{
    public class TestUserService : NextApiEntityService<TestUserDTO, TestUser, int>
    {
        public TestUserService(TestUnitOfWork unitOfWork, IMapper mapper, TestUserRepository repository)
            : base(unitOfWork, mapper, repository)
        {
        }
        
    }
}
