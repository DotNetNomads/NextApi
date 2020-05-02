using AutoMapper;
using NextApi.Server.Entity;
using NextApi.TestServer.DTO;
using NextApi.TestServer.Model;

namespace NextApi.TestServer
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
