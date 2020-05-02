using System.Collections.ObjectModel;
using NextApi.Common.Tree;

namespace NextApi.TestServer.Model
{
    public class TestTreeItem : ITreeEntity<int, int?>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public virtual TestTreeItem Parent { get; set; }

        public virtual Collection<TestTreeItem> Children { get; set; }
    }
}
