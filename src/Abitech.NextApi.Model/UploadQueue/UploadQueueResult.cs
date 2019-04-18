namespace Abitech.NextApi.Model.UploadQueue
{
    /// <summary>
    /// Result of an uploaded UploadQueueDto
    /// </summary>
    public class UploadQueueResult
    {
        /// <summary>
        /// Error description, if any
        /// </summary>
        public UploadQueueError Error { get; set; }
        /// <summary>
        /// Extra (i.e. Exception message)
        /// </summary>
        public object Extra { get; set; }
    }

    /// <summary>
    /// Upload queue error enum
    /// </summary>
    public enum UploadQueueError
    {
#pragma warning disable 1591
        NoError,
        Exception,
        OutdatedChange,
        EntityDoesNotExist,
        EntityAlreadyExists,
        OnlyOneCreateOperationAllowed,
        Unknown
#pragma warning restore 1591
    }
}
