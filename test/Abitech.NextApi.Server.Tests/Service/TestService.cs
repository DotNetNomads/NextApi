using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using Abitech.NextApi.Server.Attributes;
using Abitech.NextApi.Server.Event;
using Abitech.NextApi.Server.Request;
using Abitech.NextApi.Server.Security;
using Abitech.NextApi.Server.Service;
using Abitech.NextApi.Server.Tests.Event;
using Microsoft.AspNetCore.SignalR;

#pragma warning disable 1998
namespace Abitech.NextApi.Server.Tests.Service
{
    [NextApiAnonymous]
    public class TestService : NextApiService
    {
        private INextApiUserAccessor _nextApiUserAccessor;
        private INextApiRequest _nextApiRequest;
        private INextApiEventManager _eventManager;

        public TestService(INextApiUserAccessor nextApiUserAccessor, INextApiRequest nextApiRequest, INextApiEventManager eventManager)
        {
            _nextApiUserAccessor = nextApiUserAccessor;
            _nextApiRequest = nextApiRequest;
            _eventManager = eventManager;
        }

        // tested
        public async Task<TestDTO> DtoTest(TestDTO model)
        {
            return model;
        }

        // tested
        public async Task<string> DtoAndOptionalArgTest(TestDTO model, string optionalString = "optionalString")
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            return optionalString;
        }

        // tested
        public async Task<Dictionary<string, int?>> IntegersTest(int intArg1, int? nullableIntArg2)
        {
            return new Dictionary<string, int?>
            {
                {"intArg1", intArg1},
                {"nullableIntArg2", nullableIntArg2}
            };
        }

        // tested
        public async Task<decimal> DecimalTest(decimal decimalArg1)
        {
            return decimalArg1;
        }

        // tested
        public async Task<string> StringTest(string stringArg)
        {
            return stringArg;
        }

        // tested
        public string SyncMethodTest(string stringArg)
        {
            return stringArg;
        }

        // tested
        public void SyncMethodVoidTest()
        {
        }

        // tested
        public async Task<Dictionary<string, bool?>> BoolTest(bool boolArg1, bool? nullableBoolArg2)
        {
            return new Dictionary<string, bool?>
            {
                {"boolArg1", boolArg1},
                {"nullableBoolArg2", nullableBoolArg2}
            };
        }

        // tested
        public async Task<string> MethodWithoutArgsTest()
        {
            return "Done!";
        }

        // tested
        public async Task ExceptionTest()
        {
            await Task.Delay(1000);
            throw new Exception("Sample exception");
        }

        // tested
        public async void AsyncVoidDenied()
        {
            // invalid method
        }

        public int? GetCurrentUser()
        {
            return _nextApiUserAccessor.SubjectId;
        }

        public async Task<string> UploadFile()
        {
            var file = _nextApiRequest.FilesFromClient.GetFile("belloni");
            var tempPath = Path.GetTempFileName();
            using (var fs = new FileStream(tempPath, FileMode.Open))
                await file.CopyToAsync(fs);
            return tempPath;
        }

        public async Task<NextApiFileResponse> GetFile(string path)
        {
            var fileName = "bellonicat.jpg";
            var fileStream = new FileStream(path, FileMode.Open);
            return new NextApiFileResponse(fileName, fileStream);
        }

        public async Task<NextApiFileResponse> GetFileMimeTyped(string path)
        {
            var fileName = "bellonicat.jpg";
            var fileStream = new FileStream(path, FileMode.Open);
            return new NextApiFileResponse(fileName, fileStream, "image/jpeg");
        }

        public async Task RaiseEvents()
        {
            await _eventManager.Publish<TextEvent, string>("Hello World!");
            await _eventManager.Publish<ReferenceEvent, User>(new User {Name = "Pedro!"});
            await _eventManager.Publish<WithoutPayloadEvent>();
        }
    }
#pragma warning restore 1998
}
