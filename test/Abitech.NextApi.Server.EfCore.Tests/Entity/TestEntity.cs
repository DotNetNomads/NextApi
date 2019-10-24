using System;
using Abitech.NextApi.Server.EfCore.Model.Base;

namespace Abitech.NextApi.Server.EfCore.Tests.Entity
{
    public class TestEntity: IEntity<int>, IRowGuidEnabled
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public Guid RowGuid { get; set; } = Guid.NewGuid();
    }
}
