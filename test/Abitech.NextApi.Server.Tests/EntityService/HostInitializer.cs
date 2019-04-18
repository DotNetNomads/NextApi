using System;
using System.Threading.Tasks;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Server.Tests.EntityService
{
    public static class HostInitializer
    {
        public static async Task Init(IServiceProvider services)
        {
            var context = services.GetService<TestDbContext>();

            await context.Database.EnsureCreatedAsync();

            for (int i = 1; i <= 15; i++)
            {
                await context.AddUser(i, i, i);
            }

            await context.SaveChangesAsync();
        }

        public static async Task AddUser(this TestDbContext context, int id, int? roleId, int? cityId)
        {
            var user = new TestUser
            {
                Name = $"name{id}",
                Surname = $"Surname{id}",
                Enabled = true
            };

            if (roleId != null)
            {
                if (await context.Roles.AnyAsync(r => r.RoleId == roleId))
                {
                    user.RoleId = roleId;
                }
                else
                {
                    user.Role = CreateRole(roleId.Value);
                }
            }

            if (cityId != null)
            {
                if (await context.Cities.AnyAsync(c => c.CityId == cityId))
                {
                    user.CityId = cityId;
                }
                else
                {
                    user.City = CreateCity(cityId.Value);
                }
            }

            await context.AddAsync(user);
        }

        private static TestRole CreateRole(int roleId)
        {
            return new TestRole
            {
                Name = $"roleName{roleId}"
            };
        }

        private static TestCity CreateCity(int cityId)
        {
            var rand = new Random();
            var name = $"cityName{cityId}";
            return new TestCity
            {
                Name = name,
                Population = rand.Next(),
                Demonym = name + "er"
            };
        }
    }
}
