using System;
using Abitech.NextApi.Common.Entity;
using Abitech.NextApi.Server.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Entity
{
    public class TestAuditEntity : IEntity<int>, ILoggedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? CreatedById { get; set; }
        public DateTimeOffset? Created { get; set; }
        public int? UpdatedById { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
