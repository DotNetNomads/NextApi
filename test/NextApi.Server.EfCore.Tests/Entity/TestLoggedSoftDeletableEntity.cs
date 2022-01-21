using System;
using NextApi.Common.Entity;

namespace NextApi.Server.EfCore.Tests.Entity
{
    public class TestLoggedSoftDeletableEntity : IEntity<int>, ILoggedSoftDeletableEntity<int?>, ILoggedEntity<int?>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int? CreatedById { get; set; }
        public DateTimeOffset? Created { get; set; }
        public int? UpdatedById { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public int? RemovedById { get; set; }
        public DateTimeOffset? Removed { get; set; }
        public bool IsRemoved { get; set; }
    }
}
