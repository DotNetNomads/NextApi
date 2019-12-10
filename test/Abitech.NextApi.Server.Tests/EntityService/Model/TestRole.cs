using System.ComponentModel.DataAnnotations;
using Abitech.NextApi.Server.Entity.Model;

namespace Abitech.NextApi.Server.Tests.EntityService.Model
{
    public class TestRole: IEntity<int>
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
