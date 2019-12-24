using Abitech.NextApi.Common.Entity;

namespace Abitech.NextApi.TestServer.DTO
{
    public class TestRoleDTO: IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
