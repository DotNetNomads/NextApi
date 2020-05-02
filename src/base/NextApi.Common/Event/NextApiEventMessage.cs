namespace NextApi.Common.Event
{
    /// <summary>
    /// Message received from server by NextApi client
    /// </summary>
    public class NextApiEventMessage
    {
        /// <summary>
        /// Name of occured event
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// Message payload
        /// </summary>
        public object Data { get; set; }
    }
}
