using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Abitech.NextApi.Testing;
using Abitech.NextApi.TestServer.DAL;
using Abitech.NextApi.TestServer.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.TestServerCore
{
    public static class DataInitializationHelper
    {
        public static async Task<(ITestDbContext context, IServiceScope scope)> ResolveDb(
            this INextApiApplication contextServer)
        {
            var scope = contextServer.ServerServices.CreateScope();
            var context = scope.ServiceProvider.GetService<ITestDbContext>();
            await ((DbContext)context).Database.EnsureCreatedAsync();
            return (context, scope);
        }

        public static async Task GenerateUsers(this INextApiApplication contextServer, int count = 15)
        {
            var (context, scope) = await contextServer.ResolveDb();
            var role = CreateRole(1);
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();
            var city = CreateCity();
            await context.Cities.AddAsync(city);
            await context.SaveChangesAsync();
            for (var id = 1; id <= count; id++)
            {
                var user = new TestUser
                {
                    Name = $"name{id}",
                    Surname = $"Surname{id}",
                    Enabled = true,
                    Id = id,
                    City = city,
                    Role = role
                };
                await context.Users.AddAsync(user);
            }

            await context.SaveChangesAsync();
            scope.Dispose();
        }

        public static async Task GenerateCities(this INextApiApplication contextServer, int count = 10)
        {
            var (context, scope) = await contextServer.ResolveDb();
            for (var id = 1; id <= count; id++)
            {
                var city = CreateCity();
                await context.Cities.AddAsync(city);
            }

            await context.SaveChangesAsync();
            scope.Dispose();
        }

        private static TestRole CreateRole(int roleId)
        {
            return new TestRole {Name = $"roleName{roleId}", Id = roleId};
        }

        private static TestCity CreateCity()
        {
            var rand = new Random();
            var guid = Guid.NewGuid();
            var name = $"cityName{guid}";
            return new TestCity {Name = name, Population = rand.Next(), Demonym = name + "er", Id = guid};
        }

        public static async Task GenerateTreeItems(this INextApiApplication contextServer)
        {
            var (context, scope) = await contextServer.ResolveDb();
            var mainTree = new TestTreeItem
            {
                Id = 1,
                Name = "Node1",
                ParentId = null,
                Children = new Collection<TestTreeItem>
                {
                    new TestTreeItem
                    {
                        Id = 2,
                        Name = "Node1_1",
                        ParentId = 1,
                        Children = new Collection<TestTreeItem>
                        {
                            new TestTreeItem {Id = 3, Name = "Node1_1_1", ParentId = 2}
                        }
                    },
                    new TestTreeItem
                    {
                        Id = 4,
                        Name = "Node1_2",
                        ParentId = 1,
                        Children = new Collection<TestTreeItem>
                        {
                            new TestTreeItem {Id = 5, Name = "Node1_2_1", ParentId = 4}
                        }
                    },
                    new TestTreeItem
                    {
                        Id = 6,
                        Name = "Node1_3",
                        ParentId = 1,
                        Children = new Collection<TestTreeItem>
                        {
                            new TestTreeItem {Id = 7, Name = "Node1_3_1", ParentId = 6}
                        }
                    }
                }
            };
            var sampleTestTreeItems = new List<TestTreeItem>();
            for (var i = 8; i < 38; i++)
            {
                sampleTestTreeItems.Add(new TestTreeItem {Id = i, Name = $"Node{i}"});
            }

            await context.TestTreeItems.AddAsync(mainTree);
            await context.TestTreeItems.AddRangeAsync(sampleTestTreeItems);
            await context.SaveChangesAsync();
            scope.Dispose();
        }
    }
}
