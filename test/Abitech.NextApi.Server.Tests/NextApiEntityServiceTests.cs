using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Model.Filtering;
using Abitech.NextApi.Model.Paged;
using Abitech.NextApi.Model.Tree;
using Abitech.NextApi.Server.Tests.Common;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.Tests.EntityService.DTO;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiEntityServiceTests : NextApiTest
    {
        [Theory]
        [InlineData(false, false, NextApiTransport.Http)]
        [InlineData(true, false, NextApiTransport.Http)]
        [InlineData(false, true, NextApiTransport.Http)]
        [InlineData(true, true, NextApiTransport.Http)]
        [InlineData(false, false, NextApiTransport.SignalR)]
        [InlineData(true, false, NextApiTransport.SignalR)]
        [InlineData(false, true, NextApiTransport.SignalR)]
        [InlineData(true, true, NextApiTransport.SignalR)]
        public async Task Create(bool createCity, bool createRole, NextApiTransport transport)
        {
            var userName = $"createUser_role_{createRole}_city_{createCity}";
            var user = new TestUserDTO
            {
                City = createCity
                    ? new TestCityDTO() {Name = "cityCreatedWithUser"}
                    : null,
                Role = createRole
                    ? new TestRoleDTO() {Name = "roleCreatedWithUser"}
                    : null,
                Name = userName,
                Surname = "surname!",
                Enabled = true,
                Email = "email@mail.com"
            };
            var client = await GetServiceClient(transport);
            var createdUser = await client.Create(user);
            Assert.True(createdUser.Id > 0);
            Assert.Equal(user.Name, createdUser.Name);
            if (createCity)
            {
                Assert.True(createdUser.CityId > 0);
                Assert.Equal(createdUser.CityId, createdUser.City.CityId);
            }
            else
            {
                Assert.Null(createdUser.CityId);
                Assert.Null(createdUser.City);
            }

            if (createRole)
            {
                Assert.True(createdUser.RoleId > 0);
                Assert.Equal(createdUser.RoleId, createdUser.Role.RoleId);
            }
            else
            {
                Assert.Null(createdUser.RoleId);
                Assert.Null(createdUser.Role);
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task Delete(NextApiTransport transport)
        {
            using (var scope = Factory.Server.Host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var repo = services.GetService<TestUserRepository>();
                var unitOfWork = services.GetService<TestUnitOfWork>();
                var createdUser = new TestUser()
                {
                    Name = "petyaTest", Email = "petya@mail.ru", Enabled = true, Surname = "Ivanov"
                };
                await repo.AddAsync(createdUser);
                await unitOfWork.CommitAsync();

                var userExists = await repo.GetAll()
                    .AnyAsync(u => u.Id == createdUser.Id);
                Assert.True(userExists);
                var client = await GetServiceClient(transport);
                await client.Delete(createdUser.Id);
                var userExistsAfterDelete = await repo.GetAll()
                    .AnyAsync(u => u.Id == createdUser.Id);
                Assert.False(userExistsAfterDelete);
            }
        }

        [Theory]
        [InlineData("updatedFromTests", NextApiTransport.Http)]
        [InlineData(null, NextApiTransport.Http)]
        [InlineData("updatedFromTests", NextApiTransport.SignalR)]
        [InlineData(null, NextApiTransport.SignalR)]
        public async Task Update(string name, NextApiTransport transport)
        {
            using (var scope = Factory.Server.Host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var repo = services.GetService<TestUserRepository>();
                var mapper = services.GetService<IMapper>();
                var client = await GetServiceClient(transport);
                // data from db
                var user = await repo.GetByIdAsync(14);
                var userDTO = mapper.Map<TestUser, TestUserDTO>(user);

                // update field
                userDTO.Name = name;
                // should skip as
                // https://gitlab.abitech.kz/development/common/abitech.nextapi/issues/12
                userDTO.UnknownProperty = "someValue";
                var updatedDTO = await client.Update(14, userDTO);
                Assert.Equal(14, updatedDTO.Id);
                Assert.Equal(name, updatedDTO.Name);
                Assert.Null(updatedDTO.City);
                Assert.Null(updatedDTO.Role);
            }
        }

        [Theory]
        [InlineData(null, null, NextApiTransport.Http)]
        [InlineData(0, 5, NextApiTransport.Http)]
        [InlineData(0, 5, NextApiTransport.Http, "City", "Role")]
        [InlineData(0, 5, NextApiTransport.Http, "City")]
        [InlineData(0, 5, NextApiTransport.Http, "Role")]
        [InlineData(null, null, NextApiTransport.SignalR)]
        [InlineData(0, 5, NextApiTransport.SignalR)]
        [InlineData(0, 5, NextApiTransport.SignalR, "City", "Role")]
        [InlineData(0, 5, NextApiTransport.SignalR, "City")]
        [InlineData(0, 5, NextApiTransport.SignalR, "Role")]
        public async Task GetPaged(int? skip, int? take, NextApiTransport transport, params string[] expand)
        {
            var request = new PagedRequest {Skip = skip, Take = take, Expand = expand};
            var client = await GetServiceClient(transport);
            var result = await client.GetPaged(request);
            Assert.Equal(15, result.TotalItems);
            if (take.HasValue)
            {
                Assert.Equal(5, result.Items.Count);
            }

            result.Items.ForEach(item =>
            {
                if (expand.Contains("City"))
                {
                    Assert.NotNull(item.City);
                }
                else
                {
                    Assert.Null(item.City);
                }

                if (expand.Contains("Role"))
                {
                    Assert.NotNull(item.Role);
                }
                else
                {
                    Assert.Null(item.Role);
                }
            });
        }

        [Theory]
        [InlineData(NextApiTransport.Http, "")]
        [InlineData(NextApiTransport.Http, "City")]
        [InlineData(NextApiTransport.Http, "Role")]
        [InlineData(NextApiTransport.Http, "Role", "City")]
        [InlineData(NextApiTransport.SignalR, "")]
        [InlineData(NextApiTransport.SignalR, "City")]
        [InlineData(NextApiTransport.SignalR, "Role")]
        [InlineData(NextApiTransport.SignalR, "Role", "City")]
        public async Task GetById(NextApiTransport transport, params string[] expand)
        {
            var client = await GetServiceClient(transport);
            var user = await client.GetById(14, expand.Contains("") ? null : expand);
            Assert.Equal(14, user.Id);
            if (expand.Contains("City"))
            {
                Assert.NotNull(user.City);
            }
            else
            {
                Assert.Null(user.City);
            }

            if (expand.Contains("Role"))
            {
                Assert.NotNull(user.Role);
            }
            else
            {
                Assert.Null(user.Role);
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http, "")]
        [InlineData(NextApiTransport.Http, "City")]
        [InlineData(NextApiTransport.Http, "Role")]
        [InlineData(NextApiTransport.Http, "Role", "City")]
        [InlineData(NextApiTransport.SignalR, "")]
        [InlineData(NextApiTransport.SignalR, "City")]
        [InlineData(NextApiTransport.SignalR, "Role")]
        [InlineData(NextApiTransport.SignalR, "Role", "City")]
        public async Task GetByIds(NextApiTransport transport, params string[] expand)
        {
            var client = await GetServiceClient(transport);

            int[] idArray = new int[] {14, 12, 13};

            idArray = idArray.OrderBy(i => i).ToArray();

            var users = await client.GetByIds(idArray, expand.Contains("") ? null : expand);

            for (int i = 0; i < users.Length; i++)
            {
                Assert.Equal(idArray[i], users[i].Id);

                if (expand.Contains("City"))
                {
                    Assert.NotNull(users.ToList()[i].City);
                }
                else
                {
                    Assert.Null(users.ToList()[i].City);
                }

                if (expand.Contains("Role"))
                {
                    Assert.NotNull(users.ToList()[i].Role);
                }
                else
                {
                    Assert.Null(users.ToList()[i].Role);
                }
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task GetPagedFiltered(NextApiTransport transport)
        {
            var client = await GetServiceClient(transport);
            // filter:
            // entity => entity.Enabled == true &&
            //          (new [] {5,10,23}).Contains(entity.Id) &&
            //          (entity.Name.Contains("a") || entity.Role.Name.Contains("role"))
            var paged = new PagedRequest
            {
                Filter = new FilterBuilder()
                    .Equal("Enabled", true)
                    .In("Id", new[] {5, 10, 14})
                    .Or(f => f
                        .Contains("Name", "a")
                        .Contains("Role.Name", "role"))
                    .Build()
            };

            var data = await client.GetPaged(paged);

            Assert.True(data.TotalItems == 3);
            Assert.True(data.Items.All(e => e.Id == 5 || e.Id == 10 || e.Id == 14));
        }

        [Theory]
        [InlineData(NextApiTransport.Http, false, null, 15)]
        [InlineData(NextApiTransport.SignalR, false, null, 15)]
        [InlineData(NextApiTransport.Http, true, "name15", 1)]
        [InlineData(NextApiTransport.SignalR, true, "name15", 1)]
        [InlineData(NextApiTransport.Http, true, "5", 2)]
        [InlineData(NextApiTransport.SignalR, true, "5", 2)]
        public async Task Count(NextApiTransport transport, bool enableFilter, string filterValue,
            int shouldReturnCount)
        {
            var client = await GetServiceClient(transport);
            Filter filter = null;
            if (enableFilter)
            {
                filter = new FilterBuilder().Contains("Name", filterValue).Build();
            }

            var resultCount = await client.Count(filter);
            Assert.Equal(shouldReturnCount, resultCount);
        }

        [Theory]
        [InlineData(NextApiTransport.Http, true, "name15", new int[] {15})]
        [InlineData(NextApiTransport.SignalR, true, "name15", new int[] {15})]
        [InlineData(NextApiTransport.Http, true, "5", new int[] {5, 15})]
        [InlineData(NextApiTransport.SignalR, true, "5", new int[] {5, 15})]
        public async Task GetIdsByFilter(NextApiTransport transport, bool enableFilter, string filterValue,
            int[] shouldReturnIds)
        {
            var client = await GetServiceClient(transport);
            Filter filter = null;
            if (enableFilter)
            {
                filter = new FilterBuilder().Contains("Name", filterValue).Build();
            }

            var result = await client.GetIdsByFilter(filter);
            Assert.Equal(shouldReturnIds, result);
        }

        [Theory]
        [InlineData(NextApiTransport.SignalR, null, 3)]
        [InlineData(NextApiTransport.Http, null, 3)]
        [InlineData(NextApiTransport.SignalR, 1, 1)]
        [InlineData(NextApiTransport.Http, 1, 1)]
        public async Task TestTree(NextApiTransport transport, int? parentId, int shouldReturnCount)
        {
            var client = await GetClient(transport);
            var service = new TreeEntityService(client);

            var request = new TreeRequest() {ParentId = parentId};
            //Expand = new[] {"Children"}};

            var response = await service.GetTree(request);

            Assert.Equal(shouldReturnCount, response.FirstOrDefault()?.ChildrenCount);
        }

        private async Task<TestEntityService> GetServiceClient(NextApiTransport transport)
        {
            return _nextApiEntityService ??
                   (_nextApiEntityService = new TestEntityService(await GetClient(transport), "TestUser"));
        }

        private TestEntityService _nextApiEntityService;


        class TestEntityService : NextApiEntityService<TestUserDTO, int, INextApiClient>
        {
            public TestEntityService(INextApiClient client, string serviceName) : base(client, serviceName)
            {
            }
        }

        class TreeEntityService : NextApiEntityService<TestTreeItemDto, int, INextApiClient>
        {
            public TreeEntityService(INextApiClient client) : base(client, "TestTreeItem")
            {
            }
        }
    }
}
