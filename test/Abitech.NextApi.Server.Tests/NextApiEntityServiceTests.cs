using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Common.Filtering;
using Abitech.NextApi.Common.Paged;
using Abitech.NextApi.Common.Tree;
using Abitech.NextApi.Server.Tests.Base;
using Abitech.NextApi.TestClient;
using Abitech.NextApi.TestServer.DAL;
using Abitech.NextApi.TestServer.DTO;
using Abitech.NextApi.TestServer.Model;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiEntityServiceTests : NextApiTest<TestApplication, INextApiClient>
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
            var userService = ResolveUserService(transport);
            var userName = $"createUser_role_{createRole}_city_{createCity}";
            var user = new TestUserDTO
            {
                City = createCity
                    ? new TestCityDTO {Name = "cityCreatedWithUser"}
                    : null,
                Role = createRole
                    ? new TestRoleDTO {Name = "roleCreatedWithUser"}
                    : null,
                Name = userName,
                Surname = "surname!",
                Enabled = true,
                Email = "email@mail.com"
            };
            var insertedUser = await userService.Create(user);
            var createdUser = await userService.GetById(insertedUser.Id, new[] {"City", "Role"});
            Assert.True(createdUser.Id > 0);
            Assert.Equal(user.Name, createdUser.Name);
            if (createCity)
            {
                Assert.True(createdUser.CityId != null);
                Assert.Equal(createdUser.CityId, createdUser.City.Id);
            }
            else
            {
                Assert.Null(createdUser.CityId);
                Assert.Null(createdUser.City);
            }

            if (createRole)
            {
                Assert.True(createdUser.RoleId > 0);
                Assert.Equal(createdUser.RoleId, createdUser.Role.Id);
            }
            else
            {
                Assert.Null(createdUser.RoleId);
                Assert.Null(createdUser.Role);
            }
        }

        private ITestUserService ResolveUserService(NextApiTransport transport = NextApiTransport.SignalR) =>
            App.ResolveService<ITestUserService>(null, transport);

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task Delete(NextApiTransport transport)
        {
            using var scope = App.ServerServices.CreateScope();
            var services = scope.ServiceProvider;
            var repo = services.GetService<ITestUserRepository>();
            var unitOfWork = services.GetService<IUnitOfWork>();
            var createdUser = new TestUser()
            {
                Name = "petyaTest", Email = "petya@mail.ru", Enabled = true, Surname = "Ivanov"
            };
            await repo.AddAsync(createdUser);
            await unitOfWork.CommitAsync();

            var userExists = await repo.GetAll()
                .AnyAsync(u => u.Id == createdUser.Id);
            Assert.True(userExists);
            var userService = ResolveUserService(transport);
            await userService.Delete(createdUser.Id);
            var userExistsAfterDelete = await repo.GetAll()
                .AnyAsync(u => u.Id == createdUser.Id);
            Assert.False(userExistsAfterDelete);
        }

        [Theory]
        [InlineData("updatedFromTests", NextApiTransport.Http)]
        [InlineData(null, NextApiTransport.Http)]
        [InlineData("updatedFromTests", NextApiTransport.SignalR)]
        [InlineData(null, NextApiTransport.SignalR)]
        public async Task Update(string name, NextApiTransport transport)
        {
            await App.GenerateUsers();
            using var scope = App.ServerServices.CreateScope();
            var services = scope.ServiceProvider;
            var repo = services.GetService<ITestUserRepository>();
            var mapper = services.GetService<IMapper>();
            var userService = ResolveUserService(transport);
            // data from db
            var user = await repo.GetByIdAsync(14);
            var userDto = mapper.Map<TestUser, TestUserDTO>(user);

            // update field
            userDto.Name = name;
            // should skip as
            // https://gitlab.abitech.kz/development/common/abitech.nextapi/issues/12
            userDto.UnknownProperty = "someValue";
            var updatedDto = await userService.Update(14, userDto);
            Assert.Equal(14, updatedDto.Id);
            Assert.Equal(name, updatedDto.Name);
            Assert.Null(updatedDto.City);
            Assert.Null(updatedDto.Role);
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
            await App.GenerateUsers();
            var request = new PagedRequest {Skip = skip, Take = take, Expand = expand};
            var userService = ResolveUserService(transport);
            var result = await userService.GetPaged(request);
            Assert.Equal(15, result.TotalItems);
            if (take.HasValue) Assert.Equal(5, result.Items.Count);

            result.Items.ForEach(item =>
            {
                if (expand.Contains("City"))
                    Assert.NotNull(item.City);
                else
                    Assert.Null(item.City);

                if (expand.Contains("Role"))
                    Assert.NotNull(item.Role);
                else
                    Assert.Null(item.Role);
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
            await App.GenerateUsers();
            var userService = ResolveUserService(transport);
            var user = await userService.GetById(14, expand.Contains("") ? null : expand);
            Assert.Equal(14, user.Id);
            if (expand.Contains("City"))
                Assert.NotNull(user.City);
            else
                Assert.Null(user.City);

            if (expand.Contains("Role"))
                Assert.NotNull(user.Role);
            else
                Assert.Null(user.Role);
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
            var userService = ResolveUserService(transport);

            var idArray = new[] {14, 12, 13};

            idArray = idArray.OrderBy(i => i).ToArray();

            var users = await userService.GetByIds(idArray, expand.Contains("") ? null : expand);

            for (var i = 0; i < users.Length; i++)
            {
                Assert.Equal(idArray[i], users[i].Id);

                if (expand.Contains("City"))
                    Assert.NotNull(users.ToList()[i].City);
                else
                    Assert.Null(users.ToList()[i].City);

                if (expand.Contains("Role"))
                    Assert.NotNull(users.ToList()[i].Role);
                else
                    Assert.Null(users.ToList()[i].Role);
            }
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task GetPagedFiltered(NextApiTransport transport)
        {
            await App.GenerateUsers();
            var userService = ResolveUserService(transport);
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

            var data = await userService.GetPaged(paged);

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
            await App.GenerateUsers();
            var userService = ResolveUserService(transport);
            Filter filter = null;
            if (enableFilter)
            {
                filter = new FilterBuilder().Contains("Name", filterValue).Build();
            }

            var resultCount = await userService.Count(filter);
            Assert.Equal(shouldReturnCount, resultCount);
        }

        [Theory]
        [InlineData(NextApiTransport.Http, false, null, true)]
        [InlineData(NextApiTransport.SignalR, false, null, true)]
        [InlineData(NextApiTransport.Http, true, "name15", true)]
        [InlineData(NextApiTransport.SignalR, true, "name15", true)]
        [InlineData(NextApiTransport.Http, true, "someNonExistentName", false)]
        [InlineData(NextApiTransport.SignalR, true, "someNonExistentName", false)]
        public async Task Any(NextApiTransport transport, bool enableFilter, string filterValue,
            bool shouldReturnAny)
        {
            await App.GenerateUsers();
            var client = ResolveUserService(transport);
            Filter filter = null;
            if (enableFilter)
            {
                filter = new FilterBuilder().Contains("Name", filterValue).Build();
            }

            var resultAny = await client.Any(filter);
            Assert.Equal(shouldReturnAny, resultAny);
        }

        [Theory]
        [InlineData(NextApiTransport.Http, true, "name15", new[] {15})]
        [InlineData(NextApiTransport.SignalR, true, "name15", new[] {15})]
        [InlineData(NextApiTransport.Http, true, "5", new[] {5, 15})]
        [InlineData(NextApiTransport.SignalR, true, "5", new[] {5, 15})]
        public async Task GetIdsByFilter(NextApiTransport transport, bool enableFilter, string filterValue,
            int[] shouldReturnIds)
        {
            await App.GenerateUsers();
            var userService = ResolveUserService(transport);
            Filter filter = null;
            if (enableFilter)
            {
                filter = new FilterBuilder().Contains("Name", filterValue).Build();
            }

            var result = await userService.GetIdsByFilter(filter);
            Assert.Equal(shouldReturnIds, result);
        }

        [Theory]
        [InlineData(NextApiTransport.SignalR, null, 3)]
        [InlineData(NextApiTransport.Http, null, 3)]
        [InlineData(NextApiTransport.SignalR, 1, 1)]
        [InlineData(NextApiTransport.Http, 1, 1)]
        public async Task TestTree(NextApiTransport transport, int? parentId, int shouldReturnCount)
        {
            await App.GenerateTreeItems();
            var service = App.ResolveService<ITestTreeItemService>("1", transport);

            var request = new TreeRequest() {ParentId = parentId};
            //Expand = new[] {"Children"}};

            var response = await service.GetTree(request);

            Assert.Equal(shouldReturnCount, response.Items.FirstOrDefault()?.ChildrenCount);
        }

        [Theory]
        [InlineData(NextApiTransport.SignalR)]
        [InlineData(NextApiTransport.Http)]
        public async Task TestTreeWithFilters(NextApiTransport transport)
        {
            await App.GenerateTreeItems();
            var service = App.ResolveService<ITestTreeItemService>("1", transport);

            var request = new TreeRequest()
            {
                ParentId = null,
                PagedRequest = new PagedRequest() {Filter = new FilterBuilder().Contains("Name", "0").Build()}
            };
            var response = await service.GetTree(request);
            Assert.Equal(3, response.Items.Count);

            var request1 = new TreeRequest()
            {
                ParentId = null,
                PagedRequest = new PagedRequest()
                {
                    Filter = new FilterBuilder().Contains("Name", "node").Build(), Take = 20
                }
            };
            var response1 = await service.GetTree(request1);
            Assert.Equal(20, response1.Items.Count);
            Assert.Equal(31, response1.TotalItems);
        }
    }
}
