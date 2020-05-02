using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NextApi.Testing;
using NextApi.Testing.Data;
using NextApi.TestServer.DAL;
using NextApi.TestServer.Model;

namespace NextApi.Server.Tests.Base
{
    public static class DataHelpers
    {
        public static async Task GenerateUsers(this INextApiApplication application, int count = 15)
        {
            using var db = application.ResolveDbContext<ITestDbContext>();
            var role = CreateRole(1);
            await db.Context.Roles.AddAsync(role);
            await db.Context.SaveChangesAsync();
            var city = CreateCity();
            await db.Context.Cities.AddAsync(city);
            await db.Context.SaveChangesAsync();
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
                await db.Context.Users.AddAsync(user);
            }

            await db.Context.SaveChangesAsync();
        }

        public static async Task GenerateCities(this INextApiApplication application, int count = 10)
        {
            using var db = application.ResolveDbContext<ITestDbContext>();
            for (var id = 1; id <= count; id++)
            {
                var city = CreateCity();
                await db.Context.Cities.AddAsync(city);
            }

            await db.Context.SaveChangesAsync();
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

        public static async Task GenerateTreeItems(this INextApiApplication application)
        {
            using var db = application.ResolveDbContext<ITestDbContext>();
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

            await db.Context.TestTreeItems.AddAsync(mainTree);
            await db.Context.TestTreeItems.AddRangeAsync(sampleTestTreeItems);
            await db.Context.SaveChangesAsync();
        }
    }
}
