using System;
using System.ComponentModel.DataAnnotations;
using Abitech.NextApi.Common.Entity;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.UploadQueue.Entity;

namespace Abitech.NextApi.Server.Tests.EntityService.Model
{
    public class TestCity : IColumnLoggedEntity, IEntity<Guid>
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int? SomeNullableInt { get; set; }
        public int Population { get; set; }
        public string Demonym { get; set; }
    }
}
