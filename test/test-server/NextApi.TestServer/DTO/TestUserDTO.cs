using System;
using NextApi.Common.DTO;

namespace NextApi.TestServer.DTO
{
    public class TestUserDTO: IEntityDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public Guid? CityId { get; set; }
        public int? RoleId { get; set; }
        public DateTime Birthday { get; set; }
        public string ExtraInfo { get; set; }
        public virtual TestCityDTO City { get; set; }
        public virtual TestRoleDTO Role { get; set; }
        public string UnknownProperty { get; set; }
    }
}
