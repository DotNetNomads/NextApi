using Abitech.NextApi.Server.EfCore.Model.Base;

namespace Abitech.NextApi.Server.EfCore.Tests.Entity
{
    public class TestEntity: IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
