using System;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Model.Event.System;
using Abitech.NextApi.Server.EfCore.Service;
using Abitech.NextApi.Server.EfCore.Tests.Base;
using Abitech.NextApi.Server.EfCore.Tests.Entity;
using Abitech.NextApi.Server.EfCore.Tests.Repository;
using Abitech.NextApi.Server.Event;
using Abitech.NextApi.Server.Security;
using DeepEqual.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Abitech.NextApi.Server.EfCore.Tests
{
    public class EfCoreTest
    {
        public IServiceScope Services => GetServices();

        [Fact]
        public async Task BasicLogicCheck()
        {
            using (var scope = Services)
            {
                var provider = scope.ServiceProvider;
                var repo = provider.GetService<TestEntityRepository>();
                var unitOfWork = provider.GetService<TestUnitOfWork>();

                // check adding 
                {
                    var entityToAdd = new TestEntity() {Name = "TestEntityToAdd"};

                    await repo.AddAsync(entityToAdd);
                    await unitOfWork.CommitAsync();
                    // should contain id after commit
                    Assert.True(entityToAdd.Id > 0);
                    var entityAdded = await repo.GetByIdAsync(entityToAdd.Id);
                    entityToAdd.ShouldDeepEqual(entityAdded);
                }
                // check update
                {
                    var entityToUpdate = await repo.GetByIdAsync(1);
                    // check that previously added entity
                    Assert.Equal("TestEntityToAdd", entityToUpdate.Name);
                    var newName = "TestEntityUpdate";
                    entityToUpdate.Name = newName;
                    await repo.UpdateAsync(entityToUpdate);
                    await unitOfWork.CommitAsync();
                    var updatedEntity = await repo.GetByIdAsync(entityToUpdate.Id);
                    updatedEntity.ShouldDeepEqual(entityToUpdate);
                }
                // check delete
                {
                    Assert.True(await repo.GetAll().AnyAsync());
                    await repo.DeleteAsync(e => e.Id == 1);
                    await unitOfWork.CommitAsync();
                    Assert.False(await repo.GetAll().AnyAsync());
                }
            }
        }

        [Fact]
        public async Task CheckKeyPredicatesNotSupportedException()
        {
            using (var scope = Services)
            {
                var provider = scope.ServiceProvider;
                var repo = provider.GetService<TestEntityPredicatesRepository>();
                var unitOfWork = provider.GetService<TestUnitOfWork>();

                // create entity 
                var id = "id1";
                await repo.AddAsync(new TestEntityKeyPredicate {Id = id, Description = "testException"});
                await unitOfWork.CommitAsync();

                // should throw a NotSupportedException (cause this entity type is not implements IEntity<TKey> interface)
                await Assert.ThrowsAsync<NotSupportedException>(() => repo.GetByIdAsync(id));
            }
        }

        [Fact]
        public async Task CheckKeyPredicateArrayNotSupportedException()
        {
            using (var scope = Services)
            {
                var provider = scope.ServiceProvider;
                var repo = provider.GetService<TestEntityPredicatesRepository>();
                var unitOfWork = provider.GetService<TestUnitOfWork>();

                // create entity 
                string[] ids = {"id1", "id2"};

                await repo.AddAsync(new TestEntityKeyPredicate {Id = ids[0], Description = "testException"});

                await repo.AddAsync(new TestEntityKeyPredicate {Id = ids[1], Description = "testException"});

                await unitOfWork.CommitAsync();

                // should throw a NotSupportedException (cause this entity type is not implements IEntity<TKey> interface)
                await Assert.ThrowsAsync<NotSupportedException>(() => repo.GetByIdsAsync(ids));
            }
        }

        [Fact]
        public async Task CheckSoftDeletable()
        {
            using (var scope = Services)
            {
                var provider = scope.ServiceProvider;
                var repo = provider.GetService<TestSoftDeletableRepository>();
                var unitOfWork = provider.GetService<TestUnitOfWork>();

                var createdEntity1 = new TestSoftDeletableEntity() {Name = "name1"};
                var createdEntity2 = new TestSoftDeletableEntity() {Name = "name2"};

                await repo.AddAsync(createdEntity1);
                await repo.AddAsync(createdEntity2);
                await unitOfWork.CommitAsync();

                // check entities successfully created
                Assert.True(createdEntity1.Id > 0 && !createdEntity1.IsRemoved);
                Assert.True(createdEntity2.Id > 0 && !createdEntity2.IsRemoved);

                // check soft-deletable mechanism
                await repo.DeleteAsync(createdEntity1);
                await unitOfWork.CommitAsync();

                // check entity soft-deleted
                Assert.True(createdEntity1.IsRemoved);
                Assert.Null(await repo.GetByIdAsync(createdEntity1.Id));
                Assert.True(await repo.GetAll().CountAsync() == 1);

                // disable soft-deletable mechanism and check again
                repo.EnableSoftDeletable(false);
                Assert.NotNull(await repo.GetByIdAsync(createdEntity1.Id));
                Assert.True(await repo.GetAll().CountAsync() == 2);
            }
        }

        [Fact]
        public async Task CheckAudit()
        {
            using (var scope = Services)
            {
                var provider = scope.ServiceProvider;
                var repo = provider.GetService<TestAuditEntityRepository>();
                var unitOfWork = provider.GetService<TestUnitOfWork>();

                var entity1 = new TestAuditEntity() {Name = "name1"};

                await repo.AddAsync(entity1);
                await unitOfWork.CommitAsync();

                var entityForUpdate = await repo.GetByIdAsync(entity1.Id);
                Assert.NotNull(entityForUpdate.CreatedById);
                Assert.NotNull(entityForUpdate.Created);

                entityForUpdate.Name = "name2";

                await repo.UpdateAsync(entityForUpdate);
                await unitOfWork.CommitAsync();

                var updatedEntity = await repo.GetByIdAsync(entity1.Id);

                Assert.NotNull(updatedEntity.UpdatedById);
                Assert.NotNull(updatedEntity.Updated);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CheckColumnChangesLog(bool loggingEnabled)
        {
            using (var scope = Services)
            {
                var provider = scope.ServiceProvider;
                var repo = provider.GetService<TestColumnChangesRepo>();
                var unitOfWork = provider.GetService<TestUnitOfWork>();
                var columnChangesLogger = provider.GetService<IColumnChangesLogger>();

                // enable-or-disable logging
                columnChangesLogger.LoggingEnabled = loggingEnabled;

                var entity = new TestColumnChangesEnabledEntity {Name = $"testColumnLog{loggingEnabled}"};

                await repo.AddAsync(entity);
                await unitOfWork.CommitAsync();

                // update
                entity.Name = $"testColumnLogUpdated{loggingEnabled}";
                await repo.UpdateAsync(entity);
                await unitOfWork.CommitAsync();
                // check log
                var lastChangedOn =
                    await columnChangesLogger.GetLastChange("TestColumnChangesEnabledEntity", "Name", entity.RowGuid);
                Assert.True(loggingEnabled ? lastChangedOn.HasValue : lastChangedOn == null);
            }
        }

        [Fact]
        public async Task CheckColumnChangesLoggerDisabledForNotSupportedEntities()
        {
            using (var scope = Services)
            {
                var provider = scope.ServiceProvider;
                var repo = provider.GetService<TestEntityRepository>();
                var unitOfWork = provider.GetService<TestUnitOfWork>();
                var columnChangesLogger = provider.GetService<IColumnChangesLogger>();

                var entity = new TestEntity {Name = "lolkek2222"};

                await repo.AddAsync(entity);
                await unitOfWork.CommitAsync();

                entity.Name = "updatedNameFromTests";

                await repo.UpdateAsync(entity);
                await unitOfWork.CommitAsync();

                Assert.Null(await columnChangesLogger.GetLastChange("TestEntity", "Name", entity.RowGuid));
            }
        }

        [Fact]
        public async Task CheckTablesUpdatedEvent()
        {
            using (var scope = Services)
            {
                var provider = scope.ServiceProvider;
                var repoTestEntity = provider.GetService<TestEntityRepository>();
                var repoSoftRemoveEntity = provider.GetService<TestSoftDeletableRepository>();
                var unitOfWork = provider.GetService<TestUnitOfWork>();
                var eventManager = (TestEventManager)provider.GetService<INextApiEventManager>();
                var eventOccured = false;
                eventManager.EventOccured += (type, o) =>
                {
                    if (type == typeof(DbTablesUpdatedEvent))
                    {
                        eventOccured = o is string[] arr && arr.Length == 2 && arr.Contains("TestEntity") &&
                                       arr.Contains("TestSoftDeletableEntity");
                    }
                };

                var testEntity = new TestEntity() {Name = "Keks lol 44"};
                await repoTestEntity.AddAsync(testEntity);
                await repoSoftRemoveEntity.AddAsync(new TestSoftDeletableEntity() {Name = "blah"});
                await unitOfWork.CommitAsync();
                Assert.True(eventOccured);
            }
        }


        private IServiceScope GetServices()
        {
            if (_services != null)
                return _services.CreateScope();
            var builder = new ServiceCollection();
            builder.AddScoped<INextApiUserAccessor, TestNextApiUserAccessor>();
            builder.AddDbContext<TestDbContext>(options => { options.UseInMemoryDatabase(Guid.NewGuid().ToString()); });
            builder.AddColumnChangesLogger<TestDbContext>();
            builder.AddTransient<TestEntityRepository>();
            builder.AddTransient<TestSoftDeletableRepository>();
            builder.AddTransient<TestEntityPredicatesRepository>();
            builder.AddTransient<TestAuditEntityRepository>();
            builder.AddTransient<TestColumnChangesRepo>();
            builder.AddTransient<TestUnitOfWork>();
            builder.AddScoped<INextApiEventManager, TestEventManager>();
            _services = builder.BuildServiceProvider();

            // ensure db created
            using (var scope = _services.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var context = provider.GetService<TestDbContext>();
                context.Database.EnsureCreated();
            }

            return _services.CreateScope();
        }

        private IServiceProvider _services;
    }
}
