using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NextApi.Common;
using NextApi.Common.Abstractions;
using NextApi.Common.Abstractions.Event;
using NextApi.Common.Abstractions.Security;
using NextApi.Common.Filtering;
using NextApi.Server.Entity;
using NextApi.Server.Request;
using NextApi.TestServer.DTO;
using NextApi.TestServer.Event;

#pragma warning disable 1998
namespace NextApi.TestServer.Service
{
    public class TestService : INextApiService
    {
        private INextApiUserAccessor _nextApiUserAccessor;
        private INextApiRequest _nextApiRequest;
        private INextApiEventManager _eventManager;

        public TestService(INextApiUserAccessor nextApiUserAccessor, INextApiRequest nextApiRequest,
            INextApiEventManager eventManager)
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
            return new Dictionary<string, int?> {{"intArg1", intArg1}, {"nullableIntArg2", nullableIntArg2}};
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
            return new Dictionary<string, bool?> {{"boolArg1", boolArg1}, {"nullableBoolArg2", nullableBoolArg2}};
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

        public string GetCurrentUser()
        {
            return _nextApiUserAccessor.SubjectId;
        }

        public async Task<string> UploadFile()
        {
            var file = _nextApiRequest.FilesFromClient.GetFile("belloni");
            var tempPath = Path.GetTempFileName();
            await using (var fs = new FileStream(tempPath, FileMode.Open))
                await file.CopyToAsync(fs);
            return tempPath;
        }

        public async Task<NextApiFileResponse> GetFile(string path)
        {
            var fileName = "белонька.jpg";
            var fileStream = new FileStream(path, FileMode.Open);
            return new NextApiFileResponse(fileName, fileStream);
        }

        public async Task<NextApiFileResponse> GetFileMimeTyped(string path)
        {
            var fileName = "белонька.jpg";
            var fileStream = new FileStream(path, FileMode.Open);
            return new NextApiFileResponse(fileName, fileStream, "image/jpeg");
        }

        public async Task RaiseEvents()
        {
            await _eventManager.Publish<TextEvent, string>("Hello World!");
            await _eventManager.Publish<ReferenceEvent, User>(new User {Name = "Pedro!"});
            await _eventManager.Publish<WithoutPayloadEvent>();
        }

        public IArrayItem[] TestArraySerializationDeserialization(IArrayItem[] data) => data;

        public GuidDto[] GetByFilterTest(Filter filter)
        {
            var dtos = new List<GuidDto>
            {
                new GuidDto {GuidField = new Guid("52111957-9e88-4eac-aeca-c4e633a8f6f2")},
                new GuidDto {GuidField = new Guid("0d4598e0-2cbe-4a16-be59-44bbea8f2902")},
                new GuidDto {GuidField = Guid.NewGuid()},
                new GuidDto {GuidField = Guid.NewGuid()}
            };
            var parsedFilter = filter.ToLambdaFilter<GuidDto>();
            return dtos.Where(parsedFilter.Compile()).ToArray();
        }
    }
}
