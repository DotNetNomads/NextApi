using System;
using System.Threading.Tasks;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Common.Event;
using Abitech.NextApi.Server.Event;

namespace Abitech.NextApi.Server.EfCore.Tests.Base
{
    public class TestEventManager: INextApiEventManager
    {
        public event Action<Type, object> EventOccured; 
#pragma warning disable 1998
        public async Task Publish<TEvent, TPayload>(TPayload payload) where TEvent : BaseNextApiEvent<TPayload>
#pragma warning restore 1998
        {
            EventOccured?.Invoke(typeof(TEvent), payload);
        }

#pragma warning disable 1998
        public async Task Publish<TEvent>() where TEvent : BaseNextApiEvent
#pragma warning restore 1998
        {
            EventOccured?.Invoke(typeof(TEvent), null);
        }
    }
}
