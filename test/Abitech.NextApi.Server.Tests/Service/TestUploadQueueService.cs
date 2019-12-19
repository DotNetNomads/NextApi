using System;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Abitech.NextApi.Server.UploadQueue.ChangeTracking;
using Abitech.NextApi.Server.UploadQueue.Service;

namespace Abitech.NextApi.Server.Tests.Service
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
