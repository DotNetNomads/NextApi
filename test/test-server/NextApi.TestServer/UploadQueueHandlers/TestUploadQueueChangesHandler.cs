using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextApi.Server.UploadQueue.ChangeTracking;
using NextApi.TestServer.Model;
using NextApi.UploadQueue.Common.UploadQueue;

namespace NextApi.TestServer.UploadQueueHandlers
{
    public class TestUploadQueueChangesHandler : UploadQueueChangesHandler<TestCity>
    {
        public static Guid RejectCreateGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static string RejectCreateGuidMessage = "RejectedCreate";


        public static Guid RejectUpdateGuid = Guid.Parse("00000000-0000-0000-0000-000000000002");
        public static string RejectUpdateGuidMessage = "RejectedUpdate";
        
        public static Guid RejectDeleteGuid = Guid.Parse("00000000-0000-0000-0000-000000000003");
        public static string RejectDeleteGuidMessage = "RejectedDelete";
        
        public override Task OnBeforeCreate(TestCity entityToCreate)
        {
            if (entityToCreate.Id == RejectCreateGuid)
                throw new Exception(RejectCreateGuidMessage);
            
            return Task.CompletedTask;
        }

        public override Task OnBeforeUpdate(TestCity originalEntity, IList<UploadQueueDto> updateList)
        {
            if (originalEntity.Id == RejectUpdateGuid)
                throw new Exception(RejectUpdateGuidMessage);
            
            return Task.CompletedTask;
        }

        public override Task OnBeforeDelete(TestCity entity)
        {
            if (entity.Id == RejectDeleteGuid)
                throw new Exception(RejectDeleteGuidMessage);
            
            return Task.CompletedTask;
        }

        public override Task OnAfterCreate(TestCity entity)
        {
            throw new Exception();
        }

        public override Task OnAfterUpdate(TestCity originalEntity)
        {
            throw new Exception();
        }

        public override Task OnAfterDelete(TestCity entity)
        {
            throw new Exception();
        }
    }
}
