using System;
using System.ComponentModel.DataAnnotations;
using Abitech.NextApi.Server.EfCore.Model.Base;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Entity.Model;

namespace Abitech.NextApi.Server.Tests.EntityService.Model
{
    public class TestCity : IColumnLoggedEntity, IGuidEntity<int>
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int? SomeNullableInt { get; set; }
        public int Population { get; set; }
        public string Demonym { get; set; }
        public Guid RowGuid { get; set; } = Guid.NewGuid();
    }
}
