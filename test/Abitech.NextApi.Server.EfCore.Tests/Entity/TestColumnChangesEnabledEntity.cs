using System;
using Abitech.NextApi.Server.EfCore.Model.Base;

namespace Abitech.NextApi.Server.EfCore.Tests.Entity
{
    public class TestColumnChangesEnabledEntity: IGuidEntity<int>, IColumnLoggedEntity
    {
        public int Id { get; set; }
        public Guid RowGuid { get; set; } = Guid.NewGuid();
        
        public string Name { get; set; }
    }
}
