using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Model;
using Abitech.NextApi.Model.UploadQueue;
using Abitech.NextApi.Server.EfCore.Service;
using Abitech.NextApi.Server.Tests.EntityService;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Abitech.NextApi.Server.Tests.Service;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiUploadQueueTests
    {
        private readonly ITestOutputHelper _output;

        public NextApiUploadQueueTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CreateTwiceInSameBatchTest(NextApiTransport transport)
        {
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity = new TestCity {Name = "MyNewTestCity", Population = 123456, Demonym = "MyTestCityDemonym"};

            var createOp1 = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity),
                EntityRowGuid = newTestCity.RowGuid
            };

            var createOp2 = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity),
                EntityRowGuid = newTestCity.RowGuid
            };

            await Task.Delay(2000);

            uploadQueue.Add(createOp1);
            uploadQueue.Add(createOp2);

            var client = await NextApiTest.Instance().GetClient(transport);

            var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            Assert.Equal(uploadQueue.Count, resultDict.Count);
            Assert.Contains(resultDict, pair => pair.Value.Error == UploadQueueError.NoError);
            Assert.Contains(resultDict, pair => pair.Value.Error == UploadQueueError.OnlyOneCreateOperationAllowed);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CreateTwiceTest(NextApiTransport transport)
        {
            var testServer = NextApiTest.Instance();
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity = new TestCity {Name = "MyNewTestCity", Population = 123456, Demonym = "MyTestCityDemonym"};

            var createOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity),
                EntityRowGuid = newTestCity.RowGuid
            };

            await Task.Delay(2000);

            uploadQueue.Add(createOp);

            var client = await testServer.GetClient(transport);

            var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = testServer.Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));
                var newTestCityFromServer = await testCityRepo.GetAsync(city => city.RowGuid == newTestCity.RowGuid);

                Assert.NotNull(newTestCityFromServer);
            }

            resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            var res = resultDict[createOp.Id];
            Assert.Equal(UploadQueueError.EntityAlreadyExists, res.Error);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CreateAndUpdateTest(NextApiTransport transport)
        {
            var testServer = NextApiTest.Instance();
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity = new TestCity {Name = "MyNewTestCity", Population = 123456, Demonym = "MyTestCityDemonym"};

            var createOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity),
                EntityRowGuid = newTestCity.RowGuid
            };

            await Task.Delay(2000);

            const string updatedName = "UpdatedName";
            var updateOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Update,
                EntityName = nameof(TestCity),
                ColumnName = nameof(TestCity.Name),
                NewValue = updatedName,
                EntityRowGuid = newTestCity.RowGuid
            };

            uploadQueue.Add(createOp);
            uploadQueue.Add(updateOp);

            var client = await testServer.GetClient(transport);

            var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using var scope = testServer.Factory.Server.Host.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));
            var newTestCityFromServer = await testCityRepo.GetAsync(city => city.RowGuid == newTestCity.RowGuid);

            Assert.NotNull(newTestCityFromServer);
            Assert.Equal(updatedName, newTestCityFromServer.Name);
        }

        [Theory]
        [InlineData(true, NextApiTransport.Http)]
        [InlineData(false, NextApiTransport.Http)]
        [InlineData(true, NextApiTransport.SignalR)]
        [InlineData(false, NextApiTransport.SignalR)]
        public async Task CreateAndUpdateStressTest(bool createAndUpdateInSameBatch, NextApiTransport transport)
        {
            var testServer = NextApiTest.Instance();
            var createUploadQueue = new List<UploadQueueDto>();
            var updateUploadQueue = new List<UploadQueueDto>();
            var testCities = new List<TestCity>();

            for (var i = 0; i < 1000; i++)
            {
                var newTestCity = new TestCity
                {
                    Name = "MyNewTestCity" + i, Population = i, Demonym = "MyTestCityDemonym" + i
                };
                testCities.Add(newTestCity);

                var createOp = new UploadQueueDto
                {
                    Id = Guid.NewGuid(),
                    OccuredAt = DateTimeOffset.Now,
                    OperationType = OperationType.Create,
                    EntityName = nameof(TestCity),
                    NewValue = JsonConvert.SerializeObject(newTestCity),
                    EntityRowGuid = newTestCity.RowGuid
                };

                var updatedName = "UpdatedName" + i;
                newTestCity.Name = updatedName;
                var updateOp = new UploadQueueDto
                {
                    Id = Guid.NewGuid(),
                    OccuredAt = DateTimeOffset.Now,
                    OperationType = OperationType.Update,
                    EntityName = nameof(TestCity),
                    ColumnName = nameof(TestCity.Name),
                    NewValue = updatedName,
                    EntityRowGuid = newTestCity.RowGuid
                };

                var updatedPopulation = i + 1;
                newTestCity.Population = updatedPopulation;
                var updateOp2 = new UploadQueueDto
                {
                    Id = Guid.NewGuid(),
                    OccuredAt = DateTimeOffset.Now,
                    OperationType = OperationType.Update,
                    EntityName = nameof(TestCity),
                    ColumnName = nameof(TestCity.Population),
                    NewValue = updatedPopulation,
                    EntityRowGuid = newTestCity.RowGuid
                };

                if (createAndUpdateInSameBatch)
                {
                    createUploadQueue.Add(createOp);
                    createUploadQueue.Add(updateOp);
                    createUploadQueue.Add(updateOp2);
                }
                else
                {
                    createUploadQueue.Add(createOp);
                    updateUploadQueue.Add(updateOp);
                    updateUploadQueue.Add(updateOp2);
                }
            }

            var client = await testServer.GetClient(transport);

            var sw = new Stopwatch();
            sw.Start();
            var resultDict1 = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync",
                new NextApiArgument {Name = "uploadQueue", Value = createUploadQueue});
            _output.WriteLine(
                $"Uploading createUploadQueue took {sw.Elapsed}, {nameof(createAndUpdateInSameBatch)}: {createAndUpdateInSameBatch}");

            foreach (var keyValuePair in resultDict1)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = testServer.Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));

                foreach (var testCity in testCities)
                {
                    var testCityFromServer = await testCityRepo.GetAsync(city => city.RowGuid == testCity.RowGuid);
                    Assert.NotNull(testCityFromServer);

                    if (!createAndUpdateInSameBatch) continue;

                    Assert.Equal(testCity.Name, testCityFromServer.Name);
                    Assert.Equal(testCity.Population, testCityFromServer.Population);
                }
            }

            if (createAndUpdateInSameBatch)
                return;

            sw.Restart();
            var resultDict2 = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync",
                new NextApiArgument {Name = "uploadQueue", Value = updateUploadQueue});
            _output.WriteLine($"Uploading updateUploadQueue took {sw.Elapsed}");

            foreach (var keyValuePair in resultDict2)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = testServer.Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));

                foreach (var newTestCity in testCities)
                {
                    var newTestCityFromServer =
                        await testCityRepo.GetAsync(city => city.RowGuid == newTestCity.RowGuid);

                    Assert.NotNull(newTestCityFromServer);
                    Assert.Equal(newTestCity.Name, newTestCityFromServer.Name);
                    Assert.Equal(newTestCity.Population, newTestCityFromServer.Population);
                }
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task UpdateTest(NextApiTransport transport)
        {
            var testServer = NextApiTest.Instance();
            await testServer.GenerateCities();
            var testCityRepo = (ITestCityRepository)testServer.Factory.Server.Host.Services
                .GetService(typeof(ITestCityRepository));

            var all = testCityRepo.GetAll().ToList();
            Assert.NotEmpty(all);

            var now = DateTimeOffset.Now;
            const string newDemonym = "NewDemonym";
            var uploadQueue = all.Select(testCity => new UploadQueueDto
                {
                    Id = Guid.NewGuid(),
                    OperationType = OperationType.Update,
                    OccuredAt = now,
                    EntityName = nameof(TestCity),
                    EntityRowGuid = testCity.RowGuid,
                    ColumnName = nameof(TestCity.Demonym),
                    NewValue = $"{newDemonym}{testCity.Id}"
                })
                .ToList();

            var client = await testServer.GetClient(transport);

            var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using var scope = testServer.Factory.Server.Host.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));
            all = testCityRepo.GetAll().ToList();
            foreach (var testCity in all)
            {
                Assert.Equal($"{newDemonym}{testCity.Id}", testCity.Demonym);
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task OutdatedUpdateTest(NextApiTransport transport)
        {
            var testServer = NextApiTest.Instance();
            await testServer.GenerateCities();
            var testCityRepo =
                (ITestCityRepository)testServer.Factory.Server.Host.Services
                    .GetService(typeof(ITestCityRepository));

            var all = testCityRepo.GetAll().ToList();
            Assert.NotEmpty(all);

            var now = DateTimeOffset.Now;
            const string newDemonym = "NewDemonym";

            const string entityName = nameof(TestCity);

            var testCity = all.FirstOrDefault();

            Assert.NotNull(testCity);

            var update = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OperationType = OperationType.Update,
                OccuredAt = now,
                EntityName = entityName,
                EntityRowGuid = testCity.RowGuid,
                ColumnName = nameof(TestCity.Demonym),
                NewValue = newDemonym
            };

            var uploadQueue = new List<UploadQueueDto> {update};

            var client = await testServer.GetClient(transport);

            var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            Assert.Equal(UploadQueueError.NoError, resultDict[update.Id].Error);

            uploadQueue.Clear();

            var outdatedNow = now.Subtract(TimeSpan.FromMinutes(1));
            const string newDemonymOutdated = "NewDemonymOutdated";

            var outdatedUpdate = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OperationType = OperationType.Update,
                OccuredAt = outdatedNow,
                EntityName = entityName,
                EntityRowGuid = testCity.RowGuid,
                ColumnName = nameof(TestCity.Demonym),
                NewValue = newDemonymOutdated
            };
            uploadQueue.Add(outdatedUpdate);

            resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>("TestUploadQueue",
                "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            Assert.Equal(UploadQueueError.OutdatedChange, resultDict[outdatedUpdate.Id].Error);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task UpdateWhichDoesNotExistTest(NextApiTransport transport)
        {
            var now = DateTimeOffset.Now;
            var uploadQueue = new List<UploadQueueDto>();
            const string newDemonym = "NewDemonym";
            var update = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OperationType = OperationType.Update,
                OccuredAt = now,
                EntityName = nameof(TestCity),
                EntityRowGuid = Guid.NewGuid(),
                ColumnName = nameof(TestCity.Demonym),
                NewValue = newDemonym
            };
            uploadQueue.Add(update);

            var client = await NextApiTest.Instance().GetClient(transport);

            var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            Assert.Equal(UploadQueueError.EntityDoesNotExist, resultDict[update.Id].Error);

            using var scope = NextApiTest.Instance().Factory.Server.Host.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));
            var testCity = await testCityRepo.GetAsync(city => city.RowGuid == update.EntityRowGuid);
            Assert.Null(testCity);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CheckLastChangeTest(NextApiTransport transport)
        {
            var testServer = NextApiTest.Instance();
            await testServer.GenerateCities();
            var testCityRepo =
                (ITestCityRepository)testServer.Factory.Server.Host.Services.GetService(typeof(ITestCityRepository));

            var some = testCityRepo.GetAll().Take(10).ToList();
            Assert.NotEmpty(some);

            var uploadQueue = new List<UploadQueueDto>();
            const string entityName = nameof(TestCity);
            const string newDemonym = "NewDemonym";
            foreach (var testCity in some)
            {
                await Task.Delay(1000);
                var u = new UploadQueueDto
                {
                    Id = Guid.NewGuid(),
                    OperationType = OperationType.Update,
                    OccuredAt = DateTimeOffset.Now,
                    EntityName = entityName,
                    EntityRowGuid = testCity.RowGuid,
                    ColumnName = nameof(TestCity.Demonym),
                    NewValue = $"{newDemonym}{testCity.Id}"
                };
                uploadQueue.Add(u);
            }

            var client = await testServer.GetClient(transport);

            var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using var scope = testServer.Factory.Server.Host.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var columnChangesLogger =
                (IColumnChangesLogger)serviceProvider.GetService(typeof(IColumnChangesLogger));

            foreach (var uploadQueueDto in uploadQueue)
            {
                var lastChange = await columnChangesLogger.GetLastChange(entityName, uploadQueueDto.ColumnName,
                    uploadQueueDto.EntityRowGuid);

                Assert.Equal(uploadQueueDto.OccuredAt, lastChange);
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task DeleteTest(NextApiTransport transport)
        {
            var testServer = NextApiTest.Instance();
            await testServer.GenerateCities();
            var testCityRepo =
                (ITestCityRepository)testServer.Factory.Server.Host.Services.GetService(typeof(ITestCityRepository));

            var all = testCityRepo.GetAll().ToList();
            Assert.NotEmpty(all);

            var rand = new Random();
            var count = all.Count;

            var random1 = -1;
            var random2 = -1;

            var equal = true;
            while (equal)
            {
                var rand1 = rand.Next(count);
                var rand2 = rand.Next(count);
                if (rand1 == rand2) continue;

                equal = false;
                random1 = rand1;
                random2 = rand2;
            }

            Assert.NotEqual(-1, random1);
            Assert.NotEqual(-1, random2);

            var testCity1 = all[random1];
            var testCity2 = all[random2];

            const string entityName = nameof(TestCity);
            var now = DateTimeOffset.Now;
            var uploadQueue = new List<UploadQueueDto>();

            var delete1 = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OperationType = OperationType.Delete,
                OccuredAt = now,
                EntityName = entityName,
                EntityRowGuid = testCity1.RowGuid
            };

            var delete2 = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OperationType = OperationType.Delete,
                OccuredAt = now,
                EntityName = entityName,
                EntityRowGuid = testCity2.RowGuid,
            };

            uploadQueue.Add(delete1);
            uploadQueue.Add(delete2);

            var client = await testServer.GetClient(transport);

            var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = testServer.Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));

                var testCity1FromServer = await testCityRepo.GetAsync(city => city.RowGuid == testCity1.RowGuid);
                Assert.Null(testCity1FromServer);

                var testCity2FromServer = await testCityRepo.GetAsync(city => city.RowGuid == testCity2.RowGuid);
                Assert.Null(testCity2FromServer);
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task ChangesHandlerTest(NextApiTransport transport)
        {
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity = new TestCity
            {
                Name = "MyNewTestCity",
                Population = 123456,
                Demonym = "MyTestCityDemonym",
                RowGuid = TestUploadQueueChangesHandler.RejectCreateGuid
            };

            var createOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity),
                EntityRowGuid = TestUploadQueueChangesHandler.RejectCreateGuid
            };

            var updateOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Update,
                EntityName = nameof(TestCity),
                ColumnName = nameof(TestCity.Demonym),
                NewValue = "SomeNewDemonym",
                EntityRowGuid = TestUploadQueueChangesHandler.RejectCreateGuid
            };

            await Task.Delay(1000);

            uploadQueue.Add(createOp);
            uploadQueue.Add(updateOp);

            var client = await NextApiTest.Instance().GetClient(transport);

            {
                var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync",
                    new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

                Assert.Equal(uploadQueue.Count, resultDict.Count);
                var createOpResult = resultDict[createOp.Id];
                var updateOpResult = resultDict[updateOp.Id];

                Assert.Equal(UploadQueueError.Exception, createOpResult.Error);
                Assert.Contains(TestUploadQueueChangesHandler.RejectCreateGuidMessage, createOpResult.Extra.ToString());
                Assert.Equal(UploadQueueError.Exception, updateOpResult.Error);
                Assert.Contains(TestUploadQueueChangesHandler.RejectCreateGuidMessage, updateOpResult.Extra.ToString());
            }

            {
                newTestCity.RowGuid = TestUploadQueueChangesHandler.RejectUpdateGuid;
                createOp.NewValue = JsonConvert.SerializeObject(newTestCity);
                createOp.EntityRowGuid = newTestCity.RowGuid;
                updateOp.EntityRowGuid = newTestCity.RowGuid;

                var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync",
                    new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

                Assert.Equal(uploadQueue.Count, resultDict.Count);
                var createOpResult = resultDict[createOp.Id];
                var updateOpResult = resultDict[updateOp.Id];

                Assert.Equal(UploadQueueError.NoError, createOpResult.Error);
                Assert.Null(createOpResult.Extra);
                Assert.Equal(UploadQueueError.Exception, updateOpResult.Error);
                Assert.Contains(TestUploadQueueChangesHandler.RejectUpdateGuidMessage, updateOpResult.Extra.ToString());
            }

            {
                newTestCity.RowGuid = TestUploadQueueChangesHandler.RejectDeleteGuid;
                createOp.NewValue = JsonConvert.SerializeObject(newTestCity);
                createOp.EntityRowGuid = newTestCity.RowGuid;

                uploadQueue.Clear();
                uploadQueue.Add(createOp);

                var resultDictCreate = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync",
                    new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

                Assert.Equal(uploadQueue.Count, resultDictCreate.Count);
                var createOpResult = resultDictCreate[createOp.Id];
                Assert.Equal(UploadQueueError.NoError, createOpResult.Error);

                var deleteOp = new UploadQueueDto
                {
                    Id = Guid.NewGuid(),
                    OccuredAt = DateTimeOffset.Now,
                    OperationType = OperationType.Delete,
                    EntityName = nameof(TestCity),
                    EntityRowGuid = createOp.EntityRowGuid
                };
                uploadQueue.Clear();
                uploadQueue.Add(deleteOp);

                var resultDict = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync",
                    new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

                Assert.Equal(uploadQueue.Count, resultDict.Count);
                var deleteOpResult = resultDict[deleteOp.Id];

                Assert.Equal(UploadQueueError.Exception, deleteOpResult.Error);
                Assert.Contains(TestUploadQueueChangesHandler.RejectDeleteGuidMessage, deleteOpResult.Extra.ToString());
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task UploadNullValues(NextApiTransport transport)
        {
            var testServer = NextApiTest.Instance();
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity = new TestCity
            {
                Name = "MyNewTestCity", Population = 123456, Demonym = "MyTestCityDemonym", SomeNullableInt = 100
            };

            var createOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity),
                EntityRowGuid = newTestCity.RowGuid
            };

            uploadQueue.Add(createOp);

            var client = await testServer.GetClient(transport);

            var resultDictCreate = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            foreach (var keyValuePair in resultDictCreate)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = testServer.Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));
                var newTestCityFromServer = await testCityRepo.GetAsync(city => city.RowGuid == newTestCity.RowGuid);

                Assert.NotNull(newTestCityFromServer);
                Assert.Equal(100, newTestCityFromServer.SomeNullableInt);
            }

            var updateOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Update,
                EntityName = nameof(TestCity),
                ColumnName = nameof(TestCity.SomeNullableInt),
                NewValue = null,
                EntityRowGuid = newTestCity.RowGuid
            };

            uploadQueue.Clear();
            uploadQueue.Add(updateOp);

            var resultDictUpdate = await client.Invoke<Dictionary<Guid, UploadQueueResult>>
                ("TestUploadQueue", "ProcessAsync", new NextApiArgument {Name = "uploadQueue", Value = uploadQueue});

            foreach (var keyValuePair in resultDictUpdate)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = testServer.Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));
                var newTestCityFromServer = await testCityRepo.GetAsync(city => city.RowGuid == newTestCity.RowGuid);

                Assert.NotNull(newTestCityFromServer);
                Assert.Null(newTestCityFromServer.SomeNullableInt);
            }
        }
    }
}
