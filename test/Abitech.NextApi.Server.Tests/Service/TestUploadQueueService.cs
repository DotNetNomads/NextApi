using System;
using Abitech.NextApi.Server.EfCore.Service;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.Tests.EntityService.Model;

namespace Abitech.NextApi.Server.Tests.Service
{
    public class TestUploadQueueService : UploadQueueService<TestUnitOfWork>
    {
        public TestUploadQueueService(
            IColumnChangesLogger columnChangesLogger, 
            TestUnitOfWork unitOfWork, 
            IServiceProvider serviceProvider) : base(columnChangesLogger, unitOfWork, serviceProvider)
        {
            RegisterRepository<TestCityRepository>(nameof(TestCity));
        }
    }
}
