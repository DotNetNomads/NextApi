using System.ComponentModel.DataAnnotations;

namespace Abitech.NextApi.Server.Tests.EntityService.Model
{
    public class TestCity
    {
        [Key]
        public int CityId { get; set; }
        public string Name { get; set; }
    }
}
