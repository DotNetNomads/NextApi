using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Microsoft.EntityFrameworkCore;
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

            await context.CreateTestTreeItems();

            await context.SaveChangesAsync();
        }

        public static async Task AddUser(this TestDbContext context, int id, int? roleId, int? cityId)
        {
            var user = new TestUser {Name = $"name{id}", Surname = $"Surname{id}", Enabled = true};

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
            return new TestRole {Name = $"roleName{roleId}"};
        }

        private static TestCity CreateCity(int cityId)
        {
            var rand = new Random();
            var name = $"cityName{cityId}";
            return new TestCity {Name = name, Population = rand.Next(), Demonym = name + "er"};
        }

        public static async Task CreateTestTreeItems(this TestDbContext context)
        {
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
            for (int i = 8; i < 38; i++)
            {
                sampleTestTreeItems.Add(new TestTreeItem
                {
                    Id = i,
                    Name = $"Node{i}"
                });
            }
            await context.TestTreeItems.AddAsync(mainTree);
            await context.TestTreeItems.AddRangeAsync(sampleTestTreeItems);
        }
    }
}
