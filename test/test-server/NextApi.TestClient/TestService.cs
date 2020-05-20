using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NextApi.Client;
using NextApi.Common;
using NextApi.Common.Abstractions;
using NextApi.Common.Filtering;
using NextApi.TestServer.DTO;

namespace NextApi.TestClient
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
        Task AsyncVoidDenied();
        string GetCurrentUser();
        Task<string> UploadFile(Stream fileStream, string fileName);
        Task<NextApiFileResponse> GetFile(string path);
        Task<NextApiFileResponse> GetFileMimeTyped(string path);
        Task RaiseEvents();
        Task<IArrayItem[]> TestArraySerializationDeserialization(IArrayItem[] data);
        Task<GuidDto[]> GetByFilterTest(Filter filter);
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

        public void SyncMethodVoidTest() => InvokeService(nameof(SyncMethodVoidTest));

        public Task<Dictionary<string, bool?>> BoolTest(bool boolArg1, bool? nullableBoolArg2) =>
            InvokeService<Dictionary<string, bool?>>(nameof(BoolTest), new NextApiArgument(nameof(boolArg1), boolArg1),
                new NextApiArgument(nameof(nullableBoolArg2), nullableBoolArg2));

        public Task<string> MethodWithoutArgsTest() => InvokeService<string>(nameof(MethodWithoutArgsTest));

        public Task ExceptionTest() => InvokeService(nameof(ExceptionTest));

        public Task AsyncVoidDenied() => InvokeService(nameof(AsyncVoidDenied));

        public string GetCurrentUser() => InvokeService<string>(nameof(GetCurrentUser)).Result;

        public Task<string> UploadFile(Stream fileStream, string fileName) =>
            InvokeService<string>(nameof(UploadFile), new NextApiFileArgument("belloni", fileName, fileStream));

        public Task<NextApiFileResponse> GetFile(string path) =>
            InvokeService<NextApiFileResponse>(nameof(GetFile), new NextApiArgument(nameof(path), path));

        public Task<NextApiFileResponse> GetFileMimeTyped(string path) =>
            InvokeService<NextApiFileResponse>(nameof(GetFileMimeTyped), new NextApiArgument(nameof(path), path));

        public Task RaiseEvents() => InvokeService(nameof(RaiseEvents));

        public Task<IArrayItem[]> TestArraySerializationDeserialization(IArrayItem[] data) =>
            InvokeService<IArrayItem[]>(nameof(TestArraySerializationDeserialization),
                new NextApiArgument(nameof(data), data));

        public Task<GuidDto[]> GetByFilterTest(Filter filter) =>
            InvokeService<GuidDto[]>(nameof(GetByFilterTest), new NextApiArgument(nameof(filter), filter));
    }
}
