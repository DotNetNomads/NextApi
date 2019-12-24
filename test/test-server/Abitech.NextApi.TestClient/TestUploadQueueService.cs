using Abitech.NextApi.Client;
using Abitech.NextApi.Client.UploadQueue;
using Abitech.NextApi.UploadQueue.Common.Abstractions;

namespace Abitech.NextApi.TestClient
{
    public interface ITestUploadQueueService : IUploadQueueService
    {
    }

    public class TestUploadQueueService : UploadQueueService<INextApiClient>, ITestUploadQueueService
    {
        public TestUploadQueueService(INextApiClient client) : base(client, "TestUploadQueue")
        {
        }
    }
}
