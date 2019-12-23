using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.TestServer.DTO;
using Abitech.NextApi.TestServer.Model;
using AutoMapper;

namespace Abitech.NextApi.TestServer.Service
{
    public class TestDTOProfile: Profile
    {
        public TestDTOProfile()
        {
            this.CreateTwoWayMap<TestUser, TestUserDTO>();
            this.CreateTwoWayMap<TestRole, TestRoleDTO>();
            this.CreateTwoWayMap<TestCity, TestCityDTO>();
            this.CreateTwoWayMap<TestTreeItem, TestTreeItemDto>();
        }
    }
}
