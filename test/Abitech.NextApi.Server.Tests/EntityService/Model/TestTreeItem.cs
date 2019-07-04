using System.Collections.ObjectModel;
using Abitech.NextApi.Server.EfCore.Model.Base;

namespace Abitech.NextApi.Server.Tests.EntityService.Model
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
