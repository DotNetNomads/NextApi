using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Server.EfCore.Model.Base;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Entity.Model;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.Tests.EntityService.DAL
{
    public class TestTreeItemRepository: INextApiRepository<TestTreeItem, int>
    {
        private readonly TestDbContext _context;
        private readonly DbSet<TestTreeItem> _dbset;
        private readonly bool _isRowGuidSupported;

        public TestTreeItemRepository(TestDbContext context)
        {
            _context = context;
            _dbset = _context.TestTreeItems;
            _isRowGuidSupported = typeof(IRowGuidEnabled).IsAssignableFrom(typeof(TestTreeItem));
        }

        public Task AddAsync(object entity)
        {
            throw  new NotImplementedException();
        }

        public Task UpdateAsync(object entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(object entity)
        {
            throw new NotImplementedException();
        }

        public async Task<object> GetByRowGuid(Guid rowGuid)
        {
            if (_isRowGuidSupported)
                throw new Exception("RowGuid is not supported");

            return await GetAsync(arg => ((IRowGuidEnabled) arg).RowGuid == rowGuid);
        }

        public async Task AddAsync(TestTreeItem entity)
        {
            await _dbset.AddAsync(entity);
        }

#pragma warning disable 1998
        public async Task UpdateAsync(TestTreeItem entity)
#pragma warning restore 1998
        {
            _dbset.Update(entity);
        }

#pragma warning disable 1998
        public async Task DeleteAsync(TestTreeItem entity)
#pragma warning restore 1998
        {
            _dbset.Remove(entity);
        }

#pragma warning disable 1998
        public async Task DeleteAsync(Expression<Func<TestTreeItem, bool>> @where)
#pragma warning restore 1998
        {
            var objects = _dbset.Where(where).AsEnumerable();
            foreach (var obj in objects)
                _dbset.Remove(obj);
        }

        public async Task<TestTreeItem> GetByIdAsync(int id)
        {
            return await _dbset.FindAsync(id);
        }

        public async Task<TestTreeItem> GetAsync(Expression<Func<TestTreeItem, bool>> where)
        {
            return await _dbset.FirstOrDefaultAsync(where);
        }

        public IQueryable<TestTreeItem> GetAll()
        {
            return _dbset.AsQueryable();
        }

        public async Task<TestTreeItem[]> GetByIdsAsync(int[] ids)
        {
            return await _dbset.ToArrayAsync();
        }
    }
}
