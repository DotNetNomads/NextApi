using System;
using Abitech.NextApi.Server.UploadQueue.ChangeTracking;
using Abitech.NextApi.Server.UploadQueue.Service;
using Abitech.NextApi.TestServer.DAL;
using Abitech.NextApi.TestServer.Model;

namespace Abitech.NextApi.TestServer.Service
{
    public class TestUploadQueueService : UploadQueueService<TestUnitOfWork>
    {
        public TestUploadQueueService(
            IColumnChangesLogger columnChangesLogger, 
            TestUnitOfWork unitOfWork, 
            IServiceProvider serviceProvider) : base(columnChangesLogger, unitOfWork, serviceProvider)
        {
            RegisterRepository<TestCity, ITestCityRepository>();
        }
    }
}
