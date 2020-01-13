using System.Collections.ObjectModel;
using Abitech.NextApi.Common.DTO;

namespace Abitech.NextApi.TestServer.DTO
{
    public class TestTreeItemDto: ITreeEntityDto<int, int?>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public virtual TestTreeItemDto Parent { get; set; }
        
        public virtual Collection<TestTreeItemDto> Children { get; set; }
    }
}
