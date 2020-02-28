using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Common.Abstractions.DAL;
using Abitech.NextApi.Server.Tests.Base;
using Abitech.NextApi.Server.UploadQueue.ChangeTracking;
using Abitech.NextApi.TestClient;
using Abitech.NextApi.Testing;
using Abitech.NextApi.TestServer.Model;
using Abitech.NextApi.TestServer.UploadQueueHandlers;
using Abitech.NextApi.UploadQueue.Common.UploadQueue;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiUploadQueueTests : NextApiTest<TestApplication, INextApiClient>
    {
        private readonly ITestOutputHelper _output;

        public NextApiUploadQueueTests(ITestOutputHelper output) : base(output)
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
                EntityRowGuid = newTestCity.Id
            };

            var createOp2 = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity),
                EntityRowGuid = newTestCity.Id
            };

            await Task.Delay(2000);

            uploadQueue.Add(createOp1);
            uploadQueue.Add(createOp2);

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);

            Assert.Equal(uploadQueue.Count, resultDict.Count);
            Assert.Contains(resultDict, pair => pair.Value.Error == UploadQueueError.NoError);
            Assert.Contains(resultDict, pair => pair.Value.Error == UploadQueueError.OnlyOneCreateOperationAllowed);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CreateTwiceTest(NextApiTransport transport)
        {
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity = new TestCity {Name = "MyNewTestCity", Population = 123456, Demonym = "MyTestCityDemonym"};

            var createOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity),
                EntityRowGuid = newTestCity.Id
            };

            await Task.Delay(2000);

            uploadQueue.Add(createOp);

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = App.ServerServices.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = serviceProvider.GetService<IRepo<TestCity, Guid>>();
                var newTestCityFromServer = await testCityRepo.GetAsync(city => city.Id == newTestCity.Id);

                Assert.NotNull(newTestCityFromServer);
            }

            resultDict = await service.ProcessAsync(uploadQueue);

            var res = resultDict[createOp.Id];
            Assert.Equal(UploadQueueError.EntityAlreadyExists, res.Error);
        }
        
        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CreateWithDefaultGuidTest(NextApiTransport transport)
        {
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity1 = new TestCity
            {
                Id = Guid.Empty,
                Name = "MyNewTestCity1",
                Population = 123456,
                Demonym = "MyTestCityDemonym"
            };
            
            var newTestCity2 = new TestCity
            {
                Id = Guid.Empty,
                Name = "MyNewTestCity2",
                Population = 123456,
                Demonym = "MyTestCityDemonym"
            };

            var createOp1 = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity1),
                EntityRowGuid = newTestCity1.Id
            };

            var createOp2 = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity2),
                EntityRowGuid = newTestCity2.Id
            };

            var emptyGuidUpdateOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Update,
                EntityName = nameof(TestCity),
                ColumnName = nameof(TestCity.Name),
                NewValue = "sadkf",
                EntityRowGuid = Guid.Empty
            };

            uploadQueue.Add(createOp1);
            uploadQueue.Add(createOp2);
            uploadQueue.Add(emptyGuidUpdateOp);

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);
            
            Assert.Equal(UploadQueueError.NoError, resultDict[createOp1.Id].Error);
            Assert.Equal(UploadQueueError.NoError, resultDict[createOp2.Id].Error);
            Assert.Equal(UploadQueueError.EntityDoesNotExist, resultDict[emptyGuidUpdateOp.Id].Error);
        }
        
        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task UpdateOrDeleteWithDefaultGuidTest(NextApiTransport transport)
        {
            var uploadQueue = new List<UploadQueueDto>();

            var emptyGuidUpdateOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Update,
                EntityName = nameof(TestCity),
                ColumnName = nameof(TestCity.Name),
                NewValue = "sadkf",
                EntityRowGuid = Guid.Empty
            };

            var emptyGuidDeleteOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Delete,
                EntityName = nameof(TestCity),
                EntityRowGuid = Guid.Empty
            };

            uploadQueue.Add(emptyGuidUpdateOp);
            uploadQueue.Add(emptyGuidDeleteOp);

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);
            
            Assert.Equal(UploadQueueError.EntityDoesNotExist, resultDict[emptyGuidUpdateOp.Id].Error);
            Assert.Equal(UploadQueueError.EntityDoesNotExist, resultDict[emptyGuidDeleteOp.Id].Error);
        }

        [Theory] [InlineData(NextApiTransport.Http)] [InlineData(NextApiTransport.SignalR)]
        public async Task SilentCreateDeleteTest(NextApiTransport transport)
        {
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity = new TestCity
            {
                Id = Guid.NewGuid(),
                Name = "MyNewTestCity1",
                Population = 123456,
                Demonym = "MyTestCityDemonym"
            };

            var createOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity),
                EntityRowGuid = newTestCity.Id
            };

            var updateOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Update,
                EntityName = nameof(TestCity),
                ColumnName = nameof(TestCity.Name),
                NewValue = "sadkf",
                EntityRowGuid = newTestCity.Id
            };

            var deleteOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Delete,
                EntityName = nameof(TestCity),
                EntityRowGuid = newTestCity.Id
            };

            uploadQueue.Add(createOp);
            uploadQueue.Add(updateOp);
            uploadQueue.Add(deleteOp);

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);

            foreach (var result in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, result.Value.Error);
            }
            
            // Check for default guids as well
            createOp.EntityRowGuid = Guid.Empty;
            updateOp.EntityRowGuid = Guid.Empty;
            deleteOp.EntityRowGuid = Guid.Empty;
            
            resultDict = await service.ProcessAsync(uploadQueue);
            foreach (var result in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, result.Value.Error);
            }
        }

        private ITestUploadQueueService ResolveQueueService(NextApiTransport transport) =>
            App.ResolveService<ITestUploadQueueService>(null, transport);

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CreateAndUpdateTest(NextApiTransport transport)
        {
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity = new TestCity {Name = "MyNewTestCity", Population = 123456, Demonym = "MyTestCityDemonym"};

            var createOp = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OccuredAt = DateTimeOffset.Now,
                OperationType = OperationType.Create,
                EntityName = nameof(TestCity),
                NewValue = JsonConvert.SerializeObject(newTestCity),
                EntityRowGuid = newTestCity.Id
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
                EntityRowGuid = newTestCity.Id
            };

            uploadQueue.Add(createOp);
            uploadQueue.Add(updateOp);

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using var scope = App.ServerServices.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var testCityRepo = serviceProvider.GetService<IRepo<TestCity, Guid>>();
            var newTestCityFromServer = await testCityRepo.GetAsync(city => city.Id == newTestCity.Id);

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
            var createUploadQueue = new List<UploadQueueDto>();
            var updateUploadQueue = new List<UploadQueueDto>();
            var testCities = new List<TestCity>();

            for (var i = 0; i < 500; i++)
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
                    EntityRowGuid = newTestCity.Id
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
                    EntityRowGuid = newTestCity.Id
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
                    EntityRowGuid = newTestCity.Id
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

            var service = ResolveQueueService(transport);

            var sw = new Stopwatch();
            sw.Start();
            var resultDict1 = await service.ProcessAsync(createUploadQueue);
            _output.WriteLine(
                $"Uploading createUploadQueue took {sw.Elapsed}, {nameof(createAndUpdateInSameBatch)}: {createAndUpdateInSameBatch}");

            foreach (var keyValuePair in resultDict1) Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);

            using (var scope = App.ServerServices.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = serviceProvider.GetService<IRepo<TestCity, Guid>>();

                foreach (var testCity in testCities)
                {
                    var testCityFromServer = await testCityRepo.GetAsync(city => city.Id == testCity.Id);
                    Assert.NotNull(testCityFromServer);

                    if (!createAndUpdateInSameBatch) continue;

                    Assert.Equal(testCity.Name, testCityFromServer.Name);
                    Assert.Equal(testCity.Population, testCityFromServer.Population);
                }
            }

            if (createAndUpdateInSameBatch)
                return;

            sw.Restart();
            var resultDict2 = await service.ProcessAsync(updateUploadQueue);
            _output.WriteLine($"Uploading updateUploadQueue took {sw.Elapsed}");

            foreach (var keyValuePair in resultDict2) Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);

            using (var scope = App.ServerServices.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = serviceProvider.GetService<IRepo<TestCity, Guid>>();

                foreach (var newTestCity in testCities)
                {
                    var newTestCityFromServer =
                        await testCityRepo.GetAsync(city => city.Id == newTestCity.Id);

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
            await App.GenerateCities();
            List<TestCity> all;
            using (var serviceScope = App.ServerServices.CreateScope())
            {
                var testCityRepo = serviceScope.ServiceProvider.GetService<IRepo<TestCity, Guid>>();
                all = testCityRepo.GetAll().ToList();
            }

            Assert.NotEmpty(all);

            var now = DateTimeOffset.Now;
            const string newDemonym = "NewDemonym";
            var uploadQueue = all.Select(testCity => new UploadQueueDto
                {
                    Id = Guid.NewGuid(),
                    OperationType = OperationType.Update,
                    OccuredAt = now,
                    EntityName = nameof(TestCity),
                    EntityRowGuid = testCity.Id,
                    ColumnName = nameof(TestCity.Demonym),
                    NewValue = $"{newDemonym}{testCity.Id}"
                })
                .ToList();

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);

            foreach (var keyValuePair in resultDict) Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);

            using var scope = App.ServerServices.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var cityRepo = serviceProvider.GetService<IRepo<TestCity, Guid>>();
            all = cityRepo.GetAll().ToList();
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
            await App.GenerateCities();

            List<TestCity> all;
            using (var serviceScope = App.ServerServices.CreateScope())
            {
                var testCityRepo = serviceScope.ServiceProvider.GetService<IRepo<TestCity, Guid>>();
                all = testCityRepo.GetAll().ToList();
            }

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
                EntityRowGuid = testCity.Id,
                ColumnName = nameof(TestCity.Demonym),
                NewValue = newDemonym
            };

            var uploadQueue = new List<UploadQueueDto> {update};

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);

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
                EntityRowGuid = testCity.Id,
                ColumnName = nameof(TestCity.Demonym),
                NewValue = newDemonymOutdated
            };
            uploadQueue.Add(outdatedUpdate);

            resultDict = await service.ProcessAsync(uploadQueue);
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

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);

            Assert.Equal(UploadQueueError.EntityDoesNotExist, resultDict[update.Id].Error);

            using var scope = App.ServerServices.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var testCityRepo = serviceProvider.GetService<IRepo<TestCity, Guid>>();
            var testCity = await testCityRepo.GetAsync(city => city.Id == update.EntityRowGuid);
            Assert.Null(testCity);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CheckLastChangeTest(NextApiTransport transport)
        {
            await App.GenerateCities();
            List<TestCity> some;
            using (var serviceScope = App.ServerServices.CreateScope())
            {
                var testCityRepo = serviceScope.ServiceProvider.GetService<IRepo<TestCity, Guid>>();
                some = testCityRepo.GetAll().Take(10).ToList();
            }

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
                    OccuredAt = DateTimeOffset.UtcNow,
                    EntityName = entityName,
                    EntityRowGuid = testCity.Id,
                    ColumnName = nameof(TestCity.Demonym),
                    NewValue = $"{newDemonym}{testCity.Id}"
                };
                uploadQueue.Add(u);
            }

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);

            foreach (var keyValuePair in resultDict) Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);

            using var scope = App.ServerServices.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var columnChangesLogger = serviceProvider.GetService<IColumnChangesLogger>();

            foreach (var uploadQueueDto in uploadQueue)
            {
                var lastChange = await columnChangesLogger.GetLastChange(entityName, uploadQueueDto.ColumnName,
                    uploadQueueDto.EntityRowGuid);
                // I don't have an idea, why Equal(dt1, dt2) gives the terrible result as: 
                //    Assert.Equal() Failure
                //                Expected: 2020-01-17T13:11:20.0984364+00:00
                //                Actual:   2020-01-17T13:11:20.0984360+00:00
                Assert.Equal(
                    new DateTime(uploadQueueDto.OccuredAt.Year, uploadQueueDto.OccuredAt.Month,
                        uploadQueueDto.OccuredAt.Day, uploadQueueDto.OccuredAt.Hour, uploadQueueDto.OccuredAt.Minute,
                        uploadQueueDto.OccuredAt.Second), new DateTime(lastChange.Value.Year, lastChange.Value.Month,
                        lastChange.Value.Day, lastChange.Value.Hour, lastChange.Value.Minute,
                        lastChange.Value.Second));
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task DeleteTest(NextApiTransport transport)
        {
            await App.GenerateCities();
            List<TestCity> all;
            using (var serviceScope = App.ServerServices.CreateScope())
            {
                var testCityRepo = serviceScope.ServiceProvider.GetService<IRepo<TestCity, Guid>>();
                all = testCityRepo.GetAll().ToList();
            }

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
                EntityRowGuid = testCity1.Id
            };

            var delete2 = new UploadQueueDto
            {
                Id = Guid.NewGuid(),
                OperationType = OperationType.Delete,
                OccuredAt = now,
                EntityName = entityName,
                EntityRowGuid = testCity2.Id,
            };

            uploadQueue.Add(delete1);
            uploadQueue.Add(delete2);

            var service = ResolveQueueService(transport);

            var resultDict = await service.ProcessAsync(uploadQueue);

            foreach (var keyValuePair in resultDict) Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);

            using var scope = App.ServerServices.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var cityRepo = serviceProvider.GetService<IRepo<TestCity, Guid>>();

            var testCity1FromServer = await cityRepo.GetAsync(city => city.Id == testCity1.Id);
            Assert.Null(testCity1FromServer);

            var testCity2FromServer = await cityRepo.GetAsync(city => city.Id == testCity2.Id);
            Assert.Null(testCity2FromServer);
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
                Id = TestUploadQueueChangesHandler.RejectCreateGuid
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

            var service = ResolveQueueService(transport);
            {
                var resultDict = await service.ProcessAsync(uploadQueue);

                Assert.Equal(uploadQueue.Count, resultDict.Count);
                var createOpResult = resultDict[createOp.Id];
                var updateOpResult = resultDict[updateOp.Id];

                Assert.Equal(UploadQueueError.Exception, createOpResult.Error);
                Assert.Contains(TestUploadQueueChangesHandler.RejectCreateGuidMessage, createOpResult.Extra.ToString());
                Assert.Equal(UploadQueueError.Exception, updateOpResult.Error);
                Assert.Contains(TestUploadQueueChangesHandler.RejectCreateGuidMessage, updateOpResult.Extra.ToString());
            }

            {
                newTestCity.Id = TestUploadQueueChangesHandler.RejectUpdateGuid;
                createOp.NewValue = JsonConvert.SerializeObject(newTestCity);
                createOp.EntityRowGuid = newTestCity.Id;
                updateOp.EntityRowGuid = newTestCity.Id;

                var resultDict = await service.ProcessAsync(uploadQueue);

                Assert.Equal(uploadQueue.Count, resultDict.Count);
                var createOpResult = resultDict[createOp.Id];
                var updateOpResult = resultDict[updateOp.Id];

                Assert.Equal(UploadQueueError.NoError, createOpResult.Error);
                Assert.Null(createOpResult.Extra);
                Assert.Equal(UploadQueueError.Exception, updateOpResult.Error);
                Assert.Contains(TestUploadQueueChangesHandler.RejectUpdateGuidMessage, updateOpResult.Extra.ToString());
            }

            {
                newTestCity.Id = TestUploadQueueChangesHandler.RejectDeleteGuid;
                createOp.NewValue = JsonConvert.SerializeObject(newTestCity);
                createOp.EntityRowGuid = newTestCity.Id;

                uploadQueue.Clear();
                uploadQueue.Add(createOp);

                var resultDictCreate = await service.ProcessAsync(uploadQueue);

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

                var resultDict = await service.ProcessAsync(uploadQueue);

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
                EntityRowGuid = newTestCity.Id
            };

            uploadQueue.Add(createOp);

            var service = ResolveQueueService(transport);

            var resultDictCreate = await service.ProcessAsync(uploadQueue);

            foreach (var keyValuePair in resultDictCreate)
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);

            using (var scope = App.ServerServices.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = serviceProvider.GetService<IRepo<TestCity, Guid>>();
                var newTestCityFromServer = await testCityRepo.GetAsync(city => city.Id == newTestCity.Id);

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
                EntityRowGuid = newTestCity.Id
            };

            uploadQueue.Clear();
            uploadQueue.Add(updateOp);

            var resultDictUpdate = await service.ProcessAsync(uploadQueue);

            foreach (var keyValuePair in resultDictUpdate)
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);

            using (var scope = App.ServerServices.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = serviceProvider.GetService<IRepo<TestCity, Guid>>();
                var newTestCityFromServer = await testCityRepo.GetAsync(city => city.Id == newTestCity.Id);

                Assert.NotNull(newTestCityFromServer);
                Assert.Null(newTestCityFromServer.SomeNullableInt);
            }
        }
    }
}
