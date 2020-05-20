using System;
using NextApi.Common.Entity;

namespace NextApi.Server.EfCore.Tests.Entity
{
    public class TestAuditEntity : IEntity<int>, ILoggedEntity<int?>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? CreatedById { get; set; }
        public DateTimeOffset? Created { get; set; }
        public int? UpdatedById { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
