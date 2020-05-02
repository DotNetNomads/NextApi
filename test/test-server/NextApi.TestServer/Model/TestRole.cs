using System.ComponentModel.DataAnnotations;
using NextApi.Common.Entity;

namespace NextApi.TestServer.Model
{
    public class TestRole: IEntity<int>
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
