using System;
using NextApi.Common.DTO;

namespace NextApi.TestServer.DTO
{
    public class TestCityDTO: IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
