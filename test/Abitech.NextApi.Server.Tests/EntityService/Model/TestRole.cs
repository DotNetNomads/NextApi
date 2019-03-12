using System.ComponentModel.DataAnnotations;

namespace Abitech.NextApi.Server.Tests.EntityService.Model
{
    public class TestRole
    {
        [Key]
        public int RoleId { get; set; }
        public string Name { get; set; }
    }
}
