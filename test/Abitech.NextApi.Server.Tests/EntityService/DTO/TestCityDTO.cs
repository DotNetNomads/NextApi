using System;
using Abitech.NextApi.Common.DTO;

namespace Abitech.NextApi.Server.Tests.EntityService.DTO
{
    public class TestCityDTO: IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
