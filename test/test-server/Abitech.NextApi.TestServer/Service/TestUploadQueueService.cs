using System;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.UploadQueue.ChangeTracking;
using Abitech.NextApi.Server.UploadQueue.Service;
using Abitech.NextApi.TestServer.DAL;
using Abitech.NextApi.TestServer.Model;

namespace Abitech.NextApi.TestServer.Service
{
    public class TestUploadQueueService : UploadQueueService
    {
        public TestUploadQueueService(
            IColumnChangesLogger columnChangesLogger,
            IUnitOfWork unitOfWork,
            IServiceProvider serviceProvider) : base(columnChangesLogger, unitOfWork, serviceProvider)
        {
        }

        protected override string UploadQueueModelsAssemblyName { get; } = "Abitech.NextApi.TestServer";
    }
}
