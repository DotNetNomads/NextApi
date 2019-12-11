using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abitech.NextApi.Model.UploadQueue;

namespace Abitech.NextApi.Model.Abstractions
{
    /// <summary>
    /// Upload queue service
    /// </summary>
    public interface IUploadQueueService
    {
        /// <summary>
        /// Process queue and return result
        /// </summary>
        /// <param name="uploadQueue">List of upload queue items</param>
        /// <returns>Dictionary of upload queue results</returns>
        Task<Dictionary<Guid, UploadQueueResult>> ProcessAsync(IList<UploadQueueDto> uploadQueue);
    }
}
