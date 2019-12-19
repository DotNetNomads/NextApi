using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Common;
using Abitech.NextApi.Server.Tests.Common;
using Abitech.NextApi.Server.Tests.Event;
using Abitech.NextApi.Server.Tests.Service;
using DeepEqual.Syntax;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiBasicTests
    {
        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task DtoTest(NextApiTransport transport)
        {
            var testSrv = NextApiTest.Instance();
            var client = await testSrv.GetClient(transport);
            var dtoModel = new TestDTO
            {
                IntProperty = 1,
                BoolProperty = true,
                StringProperty = "test",
                DecimalProperty = 1.3m,
                NullableIntProperty = null,
                NullableBoolProperty = null
            };

            var resultDto = await client.Invoke<TestDTO>("Test", "DtoTest",
                new NextApiArgument {Name = "model", Value = dtoModel});
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
            var client = await NextApiTest.Instance().GetClient(transport);
            var result = await client.Invoke<string>("Test", "MethodWithoutArgsTest");
            Assert.Equal("Done!", result);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task ExceptionTest(NextApiTransport transport)
        {
            var client = await NextApiTest.Instance().GetClient(transport);
            Exception ex = null;
            try
            {
                await client.Invoke("Test", "ExceptionTest");
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
        public async Task SyncMethodVoidTest(NextApiTransport transport)
        {
            var client = await NextApiTest.Instance().GetClient(transport);
            await client.Invoke("Test", "SyncMethodVoidTest");
            Assert.True(true);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task SyncMethodTest(NextApiTransport transport)
        {
            var client = await NextApiTest.Instance().GetClient(transport);
            const string testString = "hello";
            var result = await client.Invoke<string>("Test", "SyncMethodTest",
                new NextApiArgument {Name = "stringArg", Value = testString});
            Assert.Equal(testString, result);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task AsyncVoidDenied(NextApiTransport transport)
        {
            var client = await NextApiTest.Instance().GetClient(transport);
            try
            {
                await client.Invoke("Test", "AsyncVoidDenied");
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
        public async Task BoolTest(bool? bool1, bool? bool2, NextApiTransport transport)
        {
            var client = await NextApiTest.Instance().GetClient(transport);
            var result = await client.Invoke<Dictionary<string, bool?>>("Test", "BoolTest",
                new NextApiArgument {Name = "boolArg1", Value = bool1},
                new NextApiArgument {Name = "nullableBoolArg2", Value = bool2});
            Assert.Equal(result["boolArg1"], bool1);
            Assert.Equal(result["nullableBoolArg2"], bool2);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task StringTest(NextApiTransport transport)
        {
            var clinet = await NextApiTest.Instance().GetClient(transport);
            var str = "Hello World";
            var resultStr = await clinet.Invoke<string>("Test", "StringTest",
                new NextApiArgument {Name = "stringArg", Value = str});
            Assert.Equal(str, resultStr);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task DecimalTest(NextApiTransport transport)
        {
            var client = await NextApiTest.Instance().GetClient(transport);
            var dcm = 23m;
            var resultDcm = await client.Invoke<decimal>("Test", "DecimalTest",
                new NextApiArgument {Name = "decimalArg1", Value = dcm});
            Assert.Equal(dcm, resultDcm);
        }

        [Theory]
        [InlineData(2, null, NextApiTransport.Http)]
        [InlineData(3, 4, NextApiTransport.Http)]
        [InlineData(2, null, NextApiTransport.SignalR)]
        [InlineData(3, 4, NextApiTransport.SignalR)]
        public async Task IntegersTest(int? int1, int? int2, NextApiTransport transport)
        {
            var client = await NextApiTest.Instance().GetClient(transport);
            var result = await client.Invoke<Dictionary<string, int?>>("Test", "IntegersTest",
                new NextApiArgument {Name = "intArg1", Value = int1},
                new NextApiArgument {Name = "nullableIntArg2", Value = int2});
            Assert.Equal(result["intArg1"], int1);
            Assert.Equal(result["nullableIntArg2"], int2);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task DtoAndOptionalArgTest(NextApiTransport transport)
        {
            var client = await NextApiTest.Instance().GetClient(transport);
            var dtoModel = new TestDTO
            {
                IntProperty = 1,
                BoolProperty = true,
                StringProperty = "test",
                DecimalProperty = 1.3m,
                NullableIntProperty = null,
                NullableBoolProperty = null
            };
            var defaultStringValue = "optionalString";
            // optional string
            var resultString = await client.Invoke<string>("Test", "DtoAndOptionalArgTest",
                new NextApiArgument {Name = "model", Value = dtoModel}
            );
            Assert.Equal(defaultStringValue, resultString);

            // override default string arg
            var newArg = "newString";
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
            var client = await NextApiTest.Instance().GetClient(NextApiTransport.Http);

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(baseDir, "TestData", "белонька.jpg");
            var originalBytes = File.ReadAllBytes(filePath);

            var resultFilePath = await client.Invoke<string>("Test", "UploadFile",
                new NextApiFileArgument("belloni", filePath));

            // download and check
            var fileResponse =
                await client.Invoke<NextApiFileResponse>("Test", "GetFile",
                    new NextApiArgument("path", resultFilePath));

            var bytes = await fileResponse.GetBytes();

            Assert.Equal(originalBytes, bytes);
            Assert.Equal("application/octet-stream", fileResponse.MimeType);
            Assert.Equal("белонька.jpg", fileResponse.FileName);

            // download mime typed
            var typed = await client.Invoke<NextApiFileResponse>("Test", "GetFileMimeTyped",
                new NextApiArgument("path", resultFilePath));
            Assert.Equal("image/jpeg", typed.MimeType);
        }

        [Fact]
        public async Task EventsTest()
        {
            var client = await NextApiTest.Instance().GetClient(NextApiTransport.SignalR);

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

            Func<bool> allEventsIsNotReceived =
                () => !textEventReceived || !referenceEventReceived || !withoutPayloadEventReceived;

            var maxTries = 10;
            while (allEventsIsNotReceived())
            {
                if (maxTries == 0)
                {
                    throw new Exception("Events is not working!");
                }

                await Task.Delay(1000);
                maxTries--;
            }
        }
    }
}
