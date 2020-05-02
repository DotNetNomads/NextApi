using System;
using Abitech.NextApi.UploadQueue.Common.Entity;

namespace NextApi.Server.EfCore.Tests.Entity
{
    public class TestColumnChangesEnabledEntity: IUploadQueueEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Name { get; set; }
    }
}
