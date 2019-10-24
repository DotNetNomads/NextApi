using System;

namespace Abitech.NextApi.Server.Tests.Filtering
{
    public class TestModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? Number { get; set; }
        public DateTime Date { get; set; }
        public ReferenceModel ReferenceModel { get; set; }
    }

    public class ReferenceModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
