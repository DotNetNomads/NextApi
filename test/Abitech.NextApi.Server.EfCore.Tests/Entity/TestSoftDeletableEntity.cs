using Abitech.NextApi.Server.EfCore.Model.Base;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Entity.Model;

namespace Abitech.NextApi.Server.EfCore.Tests.Entity
{
    public class TestSoftDeletableEntity: ISoftDeletableEntity, IEntity<int>
    {
        public bool IsRemoved { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
