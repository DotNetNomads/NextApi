using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Model.Paged;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.Tests.EntityService.DTO;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiEntityServiceTests : NextApiTest
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Create(bool createCity, bool createRole)
        {
            var userName = $"createUser_role_{createRole}_city_{createCity}";
            var user = new TestUserDTO
            {
                City = createCity
                    ? new TestCityDTO()
                    {
                        Name = "cityCreatedWithUser"
                    }
                    : null,
                Role = createRole
                    ? new TestRoleDTO()
                    {
                        Name = "roleCreatedWithUser"
                    }
                    : null,
                Name = userName,
                Surname = "surname!",
                Enabled = true,
                Email = "email@mail.com"
            };
            var client = await GetServiceClient();
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

        [Fact]
        public async Task Delete()
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var repo = services.GetService<TestUserRepository>();
                var userExists = await repo.GetAll()
                    .AnyAsync(u => u.Id == 15);
                Assert.True(userExists);
                var client = await GetServiceClient();
                await client.Delete(15);
                var userExistsAfterDelete = await repo.GetAll()
                    .AnyAsync(u => u.Id == 15);
                Assert.False(userExistsAfterDelete);
            }
        }

        [Theory]
        [InlineData("updatedFromTests")]
        [InlineData(null)]
        public async Task Update(string name)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var repo = services.GetService<TestUserRepository>();
                var mapper = services.GetService<IMapper>();
                var client = await GetServiceClient();
                // data from db
                var user = await repo.GetByIdAsync(14);
                var userDTO = mapper.Map<TestUser, TestUserDTO>(user);

                // update field
                userDTO.Name = name;
                var updatedDTO = await client.Update(14, userDTO);
                Assert.Equal(14, updatedDTO.Id);
                Assert.Equal(name, updatedDTO.Name);
                Assert.Null(updatedDTO.City);
                Assert.Null(updatedDTO.Role);
            }
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(0, 5)]
        [InlineData(0, 5, "City", "Role")]
        [InlineData(0, 5, "City")]
        [InlineData(0, 5, "Role")]
        public async Task GetPaged(int? skip, int? take, params string[] expand)
        {
            var request = new PagedRequest
            {
                Skip = skip,
                Take = take,
                Expand = expand
            };
            var client = await GetServiceClient();
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
        [InlineData("")]
        [InlineData("City")]
        [InlineData("Role")]
        [InlineData("Role", "City")]
        public async Task GetById(params string[] expand)
        {
            var client = await GetServiceClient();
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

        private async Task<TestEntityService> GetServiceClient()
        {
            return _nextApiEntityService ?? (_nextApiEntityService = new TestEntityService(await GetClient(), "TestUser"));
        }

        private TestEntityService _nextApiEntityService;


        class TestEntityService : NextApiEntityService<TestUserDTO, int, INextApiClient>
        {
            public TestEntityService(INextApiClient client, string serviceName) : base(client, serviceName)
            {
            }
        }
    }
}
