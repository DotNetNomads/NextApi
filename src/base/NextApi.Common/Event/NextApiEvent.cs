using System;

namespace NextApi.Common.Event
{
    /// <summary>
    /// Basic interface for NextApi event
    /// </summary>
    public interface INextApiEvent
    {
        /// <summary>
        /// Publish event to handlers
        /// </summary>
        /// <param name="payload"></param>
        void Publish(object payload = null);
    }

    /// <summary>
    /// Base class for NextApi event  without payload
    /// </summary>
    public abstract class BaseNextApiEvent : INextApiEvent
    {
        /// <summary>
        /// Subscribe to event
        /// </summary>
        /// <param name="handler"></param>
        public void Subscribe(Action handler)
        {
            EventOccured += handler;
        }

        /// <summary>
        /// Unsubscribe from event
        /// </summary>
        /// <param name="handler"></param>
        public void Unsubscribe(Action handler)
        {
            EventOccured -= handler;
        }

        private event Action EventOccured;

        /// <inheritdoc />
        public void Publish(object payload = null)
        {
            EventOccured?.Invoke();
        }
    }

    /// <summary>
    /// Basic class for NextApi event with payload
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public abstract class BaseNextApiEvent<TPayload> : INextApiEvent
    {
        /// <summary>
        /// Subscribe to event
        /// </summary>
        /// <param name="handler"></param>
        public void Subscribe(Action<TPayload> handler)
        {
            EventOccured += handler;
        }

        /// <summary>
        /// Unsubscribe from event
        /// </summary>
        /// <param name="handler"></param>
        public void Unsubscribe(Action<TPayload> handler)
        {
            EventOccured -= handler;
        }

        private event Action<TPayload> EventOccured;

        /// <inheritdoc />
        public void Publish(object payload = null)
        {
            EventOccured?.Invoke(payload != null ? (TPayload)payload : default);
        }
    }
}
