using System;
using NextApi.Common.Entity;

namespace NextApi.Server.EfCore.Tests.Entity
{
    public class TestEntity : IEntity<Guid>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}
