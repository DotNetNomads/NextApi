using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Model;
using Abitech.NextApi.Model.UploadQueue;
using Abitech.NextApi.Server.EfCore.Service;
using Abitech.NextApi.Server.Tests.Common;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiUploadQueueTests : NextApiTest
    {
        private readonly ITestOutputHelper _output;

        public NextApiUploadQueueTests(ITestOutputHelper output, ServerFactory factory) : base(factory)
        {
            _output = output;
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CreateTwiceInSameBatchTest(NextApiTransport transport)
        {
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity = new TestCity
            {
                Name = "MyNewTestCity",
                Population = 123456,
                Demonym = "MyTestCityDemonym"
            };

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

            var client = await GetClient(transport);

            var resultDict = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = uploadQueue
            });

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

            var newTestCity = new TestCity
            {
                Name = "MyNewTestCity",
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
                EntityRowGuid = newTestCity.RowGuid
            };

            await Task.Delay(2000);

            uploadQueue.Add(createOp);

            var client = await GetClient(transport);

            var resultDict = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = uploadQueue
            });

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));
                var newTestCityFromServer = await testCityRepo.GetAsync(city => city.RowGuid == newTestCity.RowGuid);

                Assert.NotNull(newTestCityFromServer);
            }

            resultDict = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = uploadQueue
            });

            var res = resultDict[createOp.Id];
            Assert.Equal(UploadQueueError.EntityAlreadyExists, res.Error);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CreateAndUpdateTest(NextApiTransport transport)
        {
            var uploadQueue = new List<UploadQueueDto>();

            var newTestCity = new TestCity
            {
                Name = "MyNewTestCity",
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

            var client = await GetClient(transport);

            var resultDict = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = uploadQueue
            });

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));
                var newTestCityFromServer = await testCityRepo.GetAsync(city => city.RowGuid == newTestCity.RowGuid);

                Assert.NotNull(newTestCityFromServer);
                Assert.Equal(updatedName, newTestCityFromServer.Name);
            }
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

            for (int i = 0; i < 1000; i++)
            {
                var newTestCity = new TestCity
                {
                    Name = "MyNewTestCity" + i,
                    Population = i,
                    Demonym = "MyTestCityDemonym" + i
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

            var client = await GetClient(transport);

            var sw = new Stopwatch();
            sw.Start();
            var resultDict1 = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = createUploadQueue
            });
            _output.WriteLine(
                $"Uploading createUploadQueue took {sw.Elapsed}, {nameof(createAndUpdateInSameBatch)}: {createAndUpdateInSameBatch}");

            foreach (var keyValuePair in resultDict1)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = Factory.Server.Host.Services.CreateScope())
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
            var resultDict2 = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = updateUploadQueue
            });
            _output.WriteLine($"Uploading updateUploadQueue took {sw.Elapsed}");

            foreach (var keyValuePair in resultDict2)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = Factory.Server.Host.Services.CreateScope())
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
            var testCityRepo = (ITestCityRepository)Factory.Server.Host.Services
                .GetService(typeof(ITestCityRepository));

            var all = testCityRepo.GetAll().ToList();
            Assert.NotEmpty(all);

            var now = DateTimeOffset.Now;
            var uploadQueue = new List<UploadQueueDto>();
            const string newDemonym = "NewDemonym";
            foreach (var testCity in all)
            {
                var u = new UploadQueueDto
                {
                    Id = Guid.NewGuid(),
                    OperationType = OperationType.Update,
                    OccuredAt = now,
                    EntityName = nameof(TestCity),
                    EntityRowGuid = testCity.RowGuid,
                    ColumnName = nameof(TestCity.Demonym),
                    NewValue = $"{newDemonym}{testCity.CityId}"
                };
                uploadQueue.Add(u);
            }

            var client = await GetClient(transport);

            var resultDict = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = uploadQueue
            });

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));
                all = testCityRepo.GetAll().ToList();
                foreach (var testCity in all)
                {
                    Assert.Equal($"{newDemonym}{testCity.CityId}", testCity.Demonym);
                }
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task OutdatedUpdateTest(NextApiTransport transport)
        {
            var testCityRepo =
                (ITestCityRepository)Factory.Server.Host.Services.GetService(typeof(ITestCityRepository));

            var all = testCityRepo.GetAll().ToList();
            Assert.NotEmpty(all);

            var now = DateTimeOffset.Now;
            const string newDemonym = "NewDemonym";

            var entityName = nameof(TestCity);

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

            var uploadQueue = new List<UploadQueueDto>();
            uploadQueue.Add(update);

            var client = await GetClient(transport);

            var resultDict = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = uploadQueue
            });

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

            resultDict = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = uploadQueue
            });

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

            var client = await GetClient(transport);

            var resultDict = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = uploadQueue
            });

            Assert.Equal(resultDict[update.Id].Error, UploadQueueError.EntityDoesNotExist);

            using (var scope = Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                var testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));
                var testCity = await testCityRepo.GetAsync(city => city.RowGuid == update.EntityRowGuid);
                Assert.Null(testCity);
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task CheckLastChangeTest(NextApiTransport transport)
        {
            var testCityRepo =
                (ITestCityRepository)Factory.Server.Host.Services.GetService(typeof(ITestCityRepository));

            var some = testCityRepo.GetAll().Take(10).ToList();
            Assert.NotEmpty(some);

            var uploadQueue = new List<UploadQueueDto>();
            var entityName = nameof(TestCity);
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
                    NewValue = $"{newDemonym}{testCity.CityId}"
                };
                uploadQueue.Add(u);
            }

            var client = await GetClient(transport);

            var resultDict = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = uploadQueue
            });

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = Factory.Server.Host.Services.CreateScope())
            {
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
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task DeleteTest(NextApiTransport transport)
        {
            var testCityRepo =
                (ITestCityRepository)Factory.Server.Host.Services.GetService(typeof(ITestCityRepository));

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

            var entityName = nameof(TestCity);
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

            var client = await GetClient(transport);

            var resultDict = await client.Invoke<ConcurrentDictionary<Guid, UploadQueueResult>>
            ("TestUploadQueue", "ProcessAsync", new NextApiArgument
            {
                Name = "uploadQueue",
                Value = uploadQueue
            });

            foreach (var keyValuePair in resultDict)
            {
                Assert.Equal(UploadQueueError.NoError, keyValuePair.Value.Error);
            }

            using (var scope = Factory.Server.Host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                testCityRepo = (ITestCityRepository)serviceProvider.GetService(typeof(ITestCityRepository));

                var testCity1FromServer = await testCityRepo.GetAsync(city => city.RowGuid == testCity1.RowGuid);
                Assert.Null(testCity1FromServer);

                var testCity2FromServer = await testCityRepo.GetAsync(city => city.RowGuid == testCity2.RowGuid);
                Assert.Null(testCity2FromServer);
            }
        }
    }
}
