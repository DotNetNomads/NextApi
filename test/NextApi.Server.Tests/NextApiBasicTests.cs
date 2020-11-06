using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NextApi.TestClient;
using NextApi.TestServer.DTO;
using NextApi.TestServer.Event;
using DeepEqual.Syntax;
using NextApi.Client;
using NextApi.Common;
using NextApi.Common.Filtering;
using NextApi.Common.Serialization;
using NextApi.Server.Tests.Base;
using NextApi.Testing;
using Xunit;
using Xunit.Abstractions;

namespace NextApi.Server.Tests
{
    public class NextApiBasicTests : NextApiTest<TestApplication, INextApiClient>
    {
        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task DtoTest(NextApiTransport transport)
        {
            var service = ResolveTestService(transport);
            var client = App.ResolveClient(null, transport);
            var dtoModel = new TestDTO
            {
                IntProperty = 1,
                BoolProperty = true,
                StringProperty = "test",
                DecimalProperty = 1.3m,
                NullableIntProperty = null,
                NullableBoolProperty = null
            };

            var resultDto = await service.DtoTest(dtoModel);
            resultDto.IsDeepEqual(dtoModel);

            // incorrect call
            try
            {
                await client.Invoke<TestDTO>("Test", "DtoTest",
                    new NextApiArgument {Name = "model", Value = 4545433453453});
                Assert.False(true);
            }
            catch
            {
                // ignored
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task MethodWithoutArgsTest(NextApiTransport transport)
        {
            var result = await ResolveTestService(transport).MethodWithoutArgsTest();
            Assert.Equal("Done!", result);
        }

        [Theory]
        [InlineData(NextApiTransport.Http, SerializationType.Json)]
        [InlineData(NextApiTransport.Http, SerializationType.MessagePack)]
        [InlineData(NextApiTransport.SignalR, SerializationType.MessagePack)]
        public async Task ExceptionTest(NextApiTransport transport, SerializationType serType)
        {
            Exception ex = null;
            try
            {
                await ResolveTestService(transport, serType).ExceptionTest();
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.NotNull(ex);
            Assert.Contains("Sample exception", ex.Message);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public void SyncMethodVoidTest(NextApiTransport transport)
        {
            ResolveTestService(transport).SyncMethodVoidTest();
            Assert.True(true);
        }

        private ITestService ResolveTestService(NextApiTransport transport = NextApiTransport.Http,
            SerializationType serType = SerializationType.Json) =>
            App.ResolveService<ITestService>(null, transport, serType);

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public void SyncMethodTest(NextApiTransport transport)
        {
            const string testString = "hello";
            var result = ResolveTestService(transport).SyncMethodTest(testString);
            Assert.Equal(testString, result);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task AsyncVoidDenied(NextApiTransport transport)
        {
            try
            {
                await ResolveTestService(transport).AsyncVoidDenied();
                Assert.False(true, "should threw exception if invoking method has 'async void' definition");
            }
            catch (Exception ex)
            {
                Assert.Contains("Invalid definition of method", ex.Message);
            }
        }

        [Theory]
        [InlineData(true, null, NextApiTransport.Http)]
        [InlineData(true, true, NextApiTransport.Http)]
        [InlineData(true, null, NextApiTransport.SignalR)]
        [InlineData(true, true, NextApiTransport.SignalR)]
        public async Task BoolTest(bool bool1, bool? bool2, NextApiTransport transport)
        {
            var result = await ResolveTestService(transport).BoolTest(bool1, bool2);
            Assert.Equal(result["boolArg1"], bool1);
            Assert.Equal(result["nullableBoolArg2"], bool2);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task StringTest(NextApiTransport transport)
        {
            const string str = "Hello World";
            var resultStr = await ResolveTestService(transport).StringTest(str);
            Assert.Equal(str, resultStr);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task DecimalTest(NextApiTransport transport)
        {
            const decimal dcm = 23m;
            var resultDcm = await ResolveTestService(transport).DecimalTest(dcm);
            Assert.Equal(dcm, resultDcm);
        }

        [Theory]
        [InlineData(2, null, NextApiTransport.Http)]
        [InlineData(3, 4, NextApiTransport.Http)]
        [InlineData(2, null, NextApiTransport.SignalR)]
        [InlineData(3, 4, NextApiTransport.SignalR)]
        public async Task IntegersTest(int? int1, int? int2, NextApiTransport transport)
        {
            var result = await ResolveTestService(transport).IntegersTest(int1.Value, int2);
            Assert.Equal(result["intArg1"], int1);
            Assert.Equal(result["nullableIntArg2"], int2);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task DtoAndOptionalArgTest(NextApiTransport transport)
        {
            var client = App.ResolveClient(null, transport);
            var dtoModel = new TestDTO
            {
                IntProperty = 1,
                BoolProperty = true,
                StringProperty = "test",
                DecimalProperty = 1.3m,
                NullableIntProperty = null,
                NullableBoolProperty = null
            };
            const string defaultStringValue = "optionalString";
            // optional string
            var resultString = await client.Invoke<string>("Test", "DtoAndOptionalArgTest",
                new NextApiArgument {Name = "model", Value = dtoModel}
            );
            Assert.Equal(defaultStringValue, resultString);

            // override default string arg
            const string newArg = "newString";
            var newResult = await client.Invoke<string>("Test", "DtoAndOptionalArgTest",
                new NextApiArgument {Name = "model", Value = dtoModel},
                new NextApiArgument() {Name = "optionalString", Value = newArg}
            );
            Assert.Equal(newArg, newResult);
        }

        [Fact]
        public async Task UploadFileAndDownloadTest()
        {
            // upload only for http
            var service = ResolveTestService();

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(baseDir, "TestData", "белонька.jpg");
            var originalBytes = File.ReadAllBytes(filePath);

            var resultFilePath = await service.UploadFile(new MemoryStream(originalBytes), "белонька.jpg");

            // download and check
            var fileResponse = await service.GetFile(resultFilePath);

            var bytes = await fileResponse.GetBytes();

            Assert.Equal(originalBytes, bytes);
            Assert.Equal("application/octet-stream", fileResponse.MimeType);
            Assert.Equal("белонька.jpg", fileResponse.FileName);

            // download mime typed
            var typed = await service.GetFileMimeTyped(resultFilePath);
            Assert.Equal("image/jpeg", typed.MimeType);
        }

        [Fact]
        public async Task EventsTest()
        {
            // only signalr
            var client = App.ResolveClient(null, NextApiTransport.SignalR);

            var textEventReceived = false;
            var referenceEventReceived = false;
            var withoutPayloadEventReceived = false;

            client.GetEvent<TextEvent>().Subscribe(s =>
            {
                Assert.Equal("Hello World!", s);
                textEventReceived = true;
            });
            client.GetEvent<ReferenceEvent>().Subscribe(user =>
            {
                Assert.Equal("Pedro!", user.Name);
                referenceEventReceived = true;
            });
            client.GetEvent<WithoutPayloadEvent>().Subscribe(() =>
            {
                withoutPayloadEventReceived = true;
            });

            // we should ask server to raise events to client :)
            await client.Invoke("Test", "RaiseEvents");

            bool AllEventsIsNotReceived() =>
                !textEventReceived || !referenceEventReceived || !withoutPayloadEventReceived;

            var maxTries = 10;
            while (AllEventsIsNotReceived())
            {
                if (maxTries == 0)
                {
                    throw new Exception("Events is not working!");
                }

                await Task.Delay(1000);
                maxTries--;
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task TestArraySerialization(NextApiTransport transport)
        {
            var service = App.ResolveService<ITestService>(null, transport);
            // we are going to test array serialization. sounds very simple,
            // but there were some problem with sub-type detecting in JSON Serializer...
            var source = new IArrayItem[]
            {
                new EquipmentArrayItem {Id = "Dfdfsdfsf", EquipmentName = "lolkeks"},
                new EquipmentArrayItem {Id = "Dfdfsdfsf333", EquipmentName = "lolkeks555"},
                new WorkArrayItem {Id = "Dfdfsdfsf", WorkName = "lolkeksfdsfsfsd"},
                new WorkArrayItem {Id = "Dfdfsdfsf3432432re", WorkName = "lolkeksfdsfsfsddfsdfsfserere"}
            };
            var returned = await service.TestArraySerializationDeserialization(source);
            source.ShouldDeepEqual(returned);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task TestGetByFilter(NextApiTransport transport)
        {
            var service = App.ResolveService<ITestService>(null, transport);
            var guidToFilter = new Guid("52111957-9e88-4eac-aeca-c4e633a8f6f2");
            var guidToFilter2 = new Guid("0d4598e0-2cbe-4a16-be59-44bbea8f2902");
            var filter = new FilterBuilder()
                .Equal("IntField", 3)
                .Or(fb => fb
                    .Equal("GuidField", guidToFilter)
                    .In("GuidField", new[] {guidToFilter2})
                )
                .Build();
            var result = await service.GetByFilterTest(filter);
            Assert.Equal(2, result.Length);
            new[] {guidToFilter, guidToFilter2}.ShouldDeepEqual(result.Select(i => i.GuidField).ToArray());
        }

        public NextApiBasicTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
