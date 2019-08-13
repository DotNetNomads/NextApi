using System;
using System.Threading.Tasks;
using Abitech.NextApi.Model.Event;
using Abitech.NextApi.Server.Event;

namespace Abitech.NextApi.Server.EfCore.Tests.Base
{
    public class TestEventManager: INextApiEventManager
    {
        public event Action<Type, object> EventOccured; 
        public async Task Publish<TEvent, TPayload>(TPayload payload) where TEvent : BaseNextApiEvent<TPayload>
        {
            EventOccured?.Invoke(typeof(TEvent), payload);
        }

        public async Task Publish<TEvent>() where TEvent : BaseNextApiEvent
        {
            EventOccured?.Invoke(typeof(TEvent), null);
        }
    }
}
