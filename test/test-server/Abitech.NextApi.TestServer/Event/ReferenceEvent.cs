using Abitech.NextApi.Common.Event;

namespace Abitech.NextApi.TestServer.Event
{
    public class ReferenceEvent: BaseNextApiEvent<User>
    {
        
    }

    public class User
    {
        public string Name { get; set; }
    }
}
