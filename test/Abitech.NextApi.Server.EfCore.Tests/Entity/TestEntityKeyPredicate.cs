using Abitech.NextApi.Common.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Entity
{
    // NOTE: for testing default KeyPredicate in repo
    public class TestEntityKeyPredicate : IEntity<string>
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
}
