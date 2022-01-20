using System;
using NextApi.Common.Entity;

namespace NextApi.Server.EfCore.Tests.Entity
{
    public class TestLoggedSoftDeletableEntity : IEntity<int>, ILoggedSoftDeletableEntity<int?>
    {
        public bool IsRemoved { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int? RemovedById { get; set; }
        public DateTimeOffset? Removed { get; set; }
    }
}
