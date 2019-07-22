using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using Abitech.NextApi.Model.Abstractions;
using Abitech.NextApi.Model.UploadQueue;

namespace Abitech.NextApi.Client
{
    /// <summary>
    /// Client-side implementation for upload queue service
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class UploadQueueService<TClient> : NextApiService<TClient>, IUploadQueueService
        where TClient : class, INextApiClient
    {
        /// <inheritdoc />
        public Task<IDictionary<Guid, UploadQueueResult>> ProcessAsync(IList<UploadQueueDto> uploadQueue)
        {
            return InvokeService<IDictionary<Guid, UploadQueueResult>>(nameof(ProcessAsync),
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
