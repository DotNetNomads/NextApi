using System;
using Abitech.NextApi.Common.Entity;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.UploadQueue.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Entity
{
    public class TestColumnChangesEnabledEntity: IEntity<Guid>, IColumnLoggedEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Name { get; set; }
    }
}
