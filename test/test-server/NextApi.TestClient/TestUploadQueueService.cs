using Abitech.NextApi.UploadQueue.Common.Abstractions;
using NextApi.Client;
using NextApi.Client.UploadQueue;

namespace NextApi.TestClient
{
    public interface ITestUploadQueueService : IUploadQueueService
    {
    }

    public class TestUploadQueueService : UploadQueueService<INextApiClient>, ITestUploadQueueService
    {
        public TestUploadQueueService(INextApiClient client) : base(client, "UploadQueue")
        {
        }
    }
}
