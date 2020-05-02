using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextApi.Common.Abstractions;
using Abitech.NextApi.UploadQueue.Common.UploadQueue;

namespace Abitech.NextApi.UploadQueue.Common.Abstractions
{
    /// <summary>
    /// Upload queue service
    /// </summary>
    public interface IUploadQueueService: INextApiService
    {
        /// <summary>
        /// Process queue and return result
        /// </summary>
        /// <param name="uploadQueue">List of upload queue items</param>
        /// <returns>Dictionary of upload queue results</returns>
        Task<Dictionary<Guid, UploadQueueResult>> ProcessAsync(IList<UploadQueueDto> uploadQueue);
    }
}
