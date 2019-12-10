using Abitech.NextApi.Model.DTO;

namespace Abitech.NextApi.Server.Tests.EntityService.DTO
{
    public class TestCityDTO: IEntityDto<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
