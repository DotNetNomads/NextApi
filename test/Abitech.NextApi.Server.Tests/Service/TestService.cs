using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Server.Attributes;
using Abitech.NextApi.Server.Request;
using Abitech.NextApi.Server.Security;
using Abitech.NextApi.Server.Service;
using Microsoft.AspNetCore.SignalR;

#pragma warning disable 1998
namespace Abitech.NextApi.Server.Tests.Service
{
    [NextApiAnonymous]
    public class TestService : NextApiService
    {
        private INextApiUserAccessor _nextApiUserAccessor;
        private INextApiRequest _nextApiRequest;

        public TestService(INextApiUserAccessor nextApiUserAccessor, INextApiRequest nextApiRequest)
        {
            _nextApiUserAccessor = nextApiUserAccessor;
            _nextApiRequest = nextApiRequest;
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
            var file = _nextApiRequest.FilesFromClient.GetFile("bellonicat");
            var tempPath = Path.GetTempFileName();
            await file.CopyToAsync(new FileStream(tempPath, FileMode.Open));
            return tempPath;
        }
    }
#pragma warning restore 1998
}
