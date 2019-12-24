using System.Collections.ObjectModel;
using Abitech.NextApi.Common.Entity;

namespace Abitech.NextApi.TestServer.Model
{
    public class TestTreeItem: IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public virtual TestTreeItem Parent { get; set; }
        
        public virtual Collection<TestTreeItem> Children { get; set; }
    }
}
