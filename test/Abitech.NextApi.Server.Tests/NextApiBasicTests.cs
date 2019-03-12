using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using Abitech.NextApi.Server.Tests.Service;
using DeepEqual.Syntax;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiBasicTests : NextApiTest
    {
        [Fact]
        public async Task DtoTest()
        {
            var client = await GetClient();
            var dtoModel = new TestDTO
            {
                IntProperty = 1,
                BoolProperty = true,
                StringProperty = "test",
                DecimalProperty = 1.3m,
                NullableIntProperty = null,
                NullableBoolProperty = null
            };

            var resultDto = await client.Invoke<TestDTO>("Test", "DtoTest", new NextApiArgument
            {
                Name = "model",
                Value = dtoModel
            });
            resultDto.IsDeepEqual(dtoModel);

            // incorrect call
            try
            {
                await client.Invoke<TestDTO>("Test", "DtoTest", new NextApiArgument
                {
                    Name = "model",
                    Value = 4545433453453
                });
                Assert.False(true);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        [Fact]
        public async Task MethodWithoutArgsTest()
        {
            var client = await GetClient();
            var result = await client.Invoke<string>("Test", "MethodWithoutArgsTest");
            Assert.Equal("Done!", result);
        }

        [Fact]
        public async Task ExceptionTest()
        {
            var client = await GetClient();
            try
            {
                await client.Invoke("Test", "ExceptionTest");
                Assert.False(true, "Should threw exception");
            }
            catch (Exception ex)
            {
                Assert.Contains("Sample exception", ex.Message);
            }
        }

        [Fact]
        public async Task SyncMethodVoidTest()
        {
            var client = await GetClient();
            await client.Invoke("Test", "SyncMethodVoidTest");
            Assert.True(true);
        }

        [Fact]
        public async Task SyncMethodTest()
        {
            var client = await GetClient();
            const string testString = "hello";
            var result = await client.Invoke<string>("Test", "SyncMethodTest",
                new NextApiArgument {Name = "stringArg", Value = testString});
            Assert.Equal(testString, result);
        }

        [Fact]
        public async Task AsynVoidDenied()
        {
            var client = await GetClient();
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
        [InlineData(true, null)]
        [InlineData(true, true)]
        public async Task BoolTest(bool? bool1, bool? bool2)
        {
            var client = await GetClient();
            var result = await client.Invoke<Dictionary<string, bool?>>("Test", "BoolTest",
                new NextApiArgument
                {
                    Name = "boolArg1",
                    Value = bool1
                }, new NextApiArgument
                {
                    Name = "nullableBoolArg2",
                    Value = bool2
                });
            Assert.Equal(result["boolArg1"], bool1);
            Assert.Equal(result["nullableBoolArg2"], bool2);
        }

        [Fact]
        public async Task StringTest()
        {
            var clinet = await GetClient();
            var str = "Hello World";
            var resultStr = await clinet.Invoke<string>("Test", "StringTest", new NextApiArgument
            {
                Name = "stringArg",
                Value = str
            });
            Assert.Equal(str, resultStr);
        }

        [Fact]
        public async Task DecimalTest()
        {
            var client = await GetClient();
            var dcm = 23m;
            var resultDcm = await client.Invoke<decimal>("Test", "DecimalTest", new NextApiArgument
            {
                Name = "decimalArg1",
                Value = dcm
            });
            Assert.Equal(dcm, resultDcm);
        }

        [Theory]
        [InlineData(2, null)]
        [InlineData(3, 4)]
        public async Task IntegersTest(int? int1, int? int2)
        {
            var client = await GetClient();
            var result = await client.Invoke<Dictionary<string, int?>>("Test", "IntegersTest",
                new NextApiArgument
                {
                    Name = "intArg1",
                    Value = int1
                }, new NextApiArgument
                {
                    Name = "nullableIntArg2",
                    Value = int2
                });
            Assert.Equal(result["intArg1"], int1);
            Assert.Equal(result["nullableIntArg2"], int2);
        }

        [Fact]
        public async Task DtoAndOptionalArgTest()
        {
            var client = await GetClient();
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
                new NextApiArgument
                {
                    Name = "model",
                    Value = dtoModel
                }
            );
            Assert.Equal(defaultStringValue, resultString);

            // override default string arg
            var newArg = "newString";
            var newResult = await client.Invoke<string>("Test", "DtoAndOptionalArgTest",
                new NextApiArgument
                {
                    Name = "model",
                    Value = dtoModel
                },
                new NextApiArgument()
                {
                    Name = "optionalString",
                    Value = newArg
                }
            );
            Assert.Equal(newArg, newResult);
        }
    }
}
