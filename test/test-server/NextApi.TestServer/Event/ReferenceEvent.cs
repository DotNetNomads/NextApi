using NextApi.Common.Event;

namespace NextApi.TestServer.Event
{
    public class ReferenceEvent: BaseNextApiEvent<User>
    {
        
    }

    public class User
    {
        public string Name { get; set; }
    }
}
