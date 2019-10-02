using System.ComponentModel.DataAnnotations;
using Abitech.NextApi.Server.EfCore.Model.Base;

namespace Abitech.NextApi.Server.Tests.EntityService.Model
{
    public class TestUser : IEntity<int>
    {
        [Key]
        public int Id { get; set; }
        // as issue #1 
        public virtual string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public int? CityId { get; set; }
        public int? RoleId { get; set; }
        public virtual TestCity City { get; set; }
        public virtual TestRole Role { get; set; }
    }
}
