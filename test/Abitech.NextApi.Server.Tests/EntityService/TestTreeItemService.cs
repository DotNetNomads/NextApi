using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Common.Tree;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Tests.EntityService.DAL;
using Abitech.NextApi.Server.Tests.EntityService.DTO;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using AutoMapper;

namespace Abitech.NextApi.Server.Tests.EntityService
{
    public class TestTreeItemService : NextApiEntityService<TestTreeItemDto, TestTreeItem, int, TestTreeItemRepository,
        TestUnitOfWork>
    {
        public TestTreeItemService(TestUnitOfWork unitOfWork, IMapper mapper, TestTreeItemRepository repository) : base(
            unitOfWork, mapper, repository)
        {
        }

#pragma warning disable 1998
        protected override async Task<Expression<Func<TestTreeItem, bool>>> ParentPredicate(object parentId)
#pragma warning restore 1998
        {
            if (parentId == null)
            {
                return entity => entity.ParentId == null;
            }

            var converted = Convert.ToInt32(parentId);
            return entity => entity.ParentId == converted;
        }

        protected override Expression<Func<TestTreeItem, TreeItem<TestTreeItem>>> SelectTreeChunk(
            IQueryable<TestTreeItem> query) =>
            entity =>
                new TreeItem<TestTreeItem>
                {
                    Entity = entity, ChildrenCount = query.Count(childEntity => childEntity.ParentId == entity.Id)
                };
    }
}
