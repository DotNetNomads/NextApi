using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Common;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.TestServer.DTO;

namespace Abitech.NextApi.TestServer.Service
{
    public interface ITestService : INextApiService
    {
        Task<TestDTO> DtoTest(TestDTO model);
        Task<string> DtoAndOptionalArgTest(TestDTO model, string optionalString = "optionalString");
        Task<Dictionary<string, int?>> IntegersTest(int intArg1, int? nullableIntArg2);
        Task<decimal> DecimalTest(decimal decimalArg1);
        Task<string> StringTest(string stringArg);
        string SyncMethodTest(string stringArg);
        void SyncMethodVoidTest();
        Task<Dictionary<string, bool?>> BoolTest(bool boolArg1, bool? nullableBoolArg2);
        Task<string> MethodWithoutArgsTest();
        Task ExceptionTest();
        void AsyncVoidDenied();
        int? GetCurrentUser();
        Task<string> UploadFile();
        Task<NextApiFileResponse> GetFile(string path);
        Task<NextApiFileResponse> GetFileMimeTyped(string path);
        Task RaiseEvents();
    }

    public class TestService : NextApiService<INextApiClient>, ITestService
    {
        public TestService(INextApiClient client) : base(client, "Test")
        {
        }

        public Task<TestDTO> DtoTest(TestDTO model) =>
            InvokeService<TestDTO>(nameof(DtoTest), new NextApiArgument(nameof(model), model));

        public Task<string> DtoAndOptionalArgTest(TestDTO model, string optionalString = "optionalString") =>
            InvokeService<string>(nameof(DtoAndOptionalArgTest), new NextApiArgument(nameof(model), model),
                new NextApiArgument(nameof(optionalString), optionalString));

        public Task<Dictionary<string, int?>> IntegersTest(int intArg1, int? nullableIntArg2) =>
            InvokeService<Dictionary<string, int?>>(nameof(IntegersTest), new NextApiArgument(nameof(intArg1), intArg1),
                new NextApiArgument(nameof(nullableIntArg2), nullableIntArg2));

        public Task<decimal> DecimalTest(decimal decimalArg1) => InvokeService<decimal>(nameof(DecimalTest),
            new NextApiArgument(nameof(decimalArg1), decimalArg1));

        public Task<string> StringTest(string stringArg) => InvokeService<string>(nameof(StringTest),
            new NextApiArgument(nameof(stringArg), stringArg));

        public string SyncMethodTest(string stringArg) => InvokeService<string>(nameof(SyncMethodTest),
            new NextApiArgument(nameof(stringArg), stringArg)).Result;

        public void SyncMethodVoidTest()
        {
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, bool?>> BoolTest(bool boolArg1, bool? nullableBoolArg2)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> MethodWithoutArgsTest()
        {
            throw new System.NotImplementedException();
        }

        public Task ExceptionTest()
        {
            throw new System.NotImplementedException();
        }

        public void AsyncVoidDenied()
        {
            throw new System.NotImplementedException();
        }

        public int? GetCurrentUser()
        {
            throw new System.NotImplementedException();
        }

        public Task<string> UploadFile()
        {
            throw new System.NotImplementedException();
        }

        public Task<NextApiFileResponse> GetFile(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task<NextApiFileResponse> GetFileMimeTyped(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task RaiseEvents()
        {
            throw new System.NotImplementedException();
        }
    }
}
