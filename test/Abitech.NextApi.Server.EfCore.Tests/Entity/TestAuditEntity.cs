using System;
using Abitech.NextApi.Server.EfCore.Model.Base;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Entity.Model;

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
