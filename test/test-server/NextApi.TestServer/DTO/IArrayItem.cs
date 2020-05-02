using MessagePack;

namespace NextApi.TestServer.DTO
{
    [Union(0, typeof(WorkArrayItem))]
    [Union(1, typeof(EquipmentArrayItem))]
    public interface IArrayItem
    {
        string Id { get; set; }
    }

    public class EquipmentArrayItem : IArrayItem
    {
        public string Id { get; set; }
        public string EquipmentName { get; set; }
    }

    public class WorkArrayItem : IArrayItem
    {
        public string Id { get; set; }
        public string WorkName { get; set; }
    }
}
