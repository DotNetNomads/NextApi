using Abitech.NextApi.Common.Entity;
using Abitech.NextApi.Server.EfCore.Entity;
using Abitech.NextApi.Server.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Entity
{
    public class TestSoftDeletableEntity: ISoftDeletableEntity, IEntity<int>
    {
        public bool IsRemoved { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
