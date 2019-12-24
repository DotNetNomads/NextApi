using System;
using Abitech.NextApi.Common.DTO;

namespace Abitech.NextApi.TestServer.DTO
{
    public class TestCityDTO: IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
