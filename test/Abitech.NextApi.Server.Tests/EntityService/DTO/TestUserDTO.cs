using Abitech.NextApi.Model.DTO;
using Abitech.NextApi.Server.Entity.Model;

namespace Abitech.NextApi.Server.Tests.EntityService.DTO
{
    public class TestUserDTO: IEntityDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public int? CityId { get; set; }
        public int? RoleId { get; set; }
        public virtual TestCityDTO City { get; set; }
        public virtual TestRoleDTO Role { get; set; }
        public string UnknownProperty { get; set; }
    }
}
