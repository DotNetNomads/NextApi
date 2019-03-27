using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Tests.EntityService.DTO;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using AutoMapper;

namespace Abitech.NextApi.Server.Tests.EntityService
{
    public class TestDTOProfile: Profile
    {
        public TestDTOProfile()
        {
            this.CreateTwoWayMap<TestUser, TestUserDTO>();
            this.CreateTwoWayMap<TestRole, TestRoleDTO>();
            this.CreateTwoWayMap<TestCity, TestCityDTO>();
        }
    }
}
