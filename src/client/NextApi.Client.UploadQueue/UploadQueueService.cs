using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextApi.UploadQueue.Common.Abstractions;
using NextApi.UploadQueue.Common.UploadQueue;
using NextApi.Common;

namespace NextApi.Client.UploadQueue
{
    /// <summary>
    /// Client-side implementation for upload queue service
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class UploadQueueService<TClient> : NextApiService<TClient>, IUploadQueueService
        where TClient : class, INextApiClient
    {
        /// <inheritdoc />
        public Task<Dictionary<Guid, UploadQueueResult>> ProcessAsync(IList<UploadQueueDto> uploadQueue)
        {
            return InvokeService<Dictionary<Guid, UploadQueueResult>>(nameof(ProcessAsync),
                new NextApiArgument(nameof(uploadQueue), uploadQueue));
        }

        /// <summary>
        /// Initialize new instance of Upload Queue service
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serviceName"></param>
        protected UploadQueueService(TClient client, string serviceName) : base(client, serviceName)
        {
        }
    }
}
