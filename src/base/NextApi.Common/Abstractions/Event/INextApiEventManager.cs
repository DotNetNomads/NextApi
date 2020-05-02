using System.Threading.Tasks;
using NextApi.Common.Event;

namespace NextApi.Common.Abstractions.Event
{
    /// <summary>
    /// Provides functionality for publishing and managing NextApi events
    /// </summary>
    public interface INextApiEventManager
    {
        /// <summary>
        /// Publish event to all connected clients
        /// </summary>
        /// <param name="payload">Event payload</param>
        /// <typeparam name="TEvent">Type of event</typeparam>
        /// <typeparam name="TPayload">Payload type</typeparam>
        /// <returns></returns>
        Task Publish<TEvent, TPayload>(TPayload payload) where TEvent : BaseNextApiEvent<TPayload>;

        /// <summary>
        /// Publish event to all connected clients (with no args)
        /// </summary>
        /// <typeparam name="TEvent">Type of event</typeparam>
        /// <returns></returns>
        Task Publish<TEvent>() where TEvent : BaseNextApiEvent;
    }
}
