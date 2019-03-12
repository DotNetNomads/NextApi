using Abitech.NextApi.Server.Tests.EntityService.DTO;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using AutoMapper;

namespace Abitech.NextApi.Server.Tests.EntityService
{
    public class TestDTOProfile: Profile
    {
        public TestDTOProfile()
        {
            CreateMap<TestUser, TestUserDTO>();
            CreateMap<TestRole, TestRoleDTO>();
            CreateMap<TestCity, TestCityDTO>();
        }
    }
}
