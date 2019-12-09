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
    public class TestUserRepository : INextApiRepository<TestUser, int>
    {
        private readonly TestDbContext _context;
        private readonly DbSet<TestUser> _dbset;
        private readonly bool _isRowGuidSupported;

        public TestUserRepository(TestDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbset = _context.Users;
            _isRowGuidSupported = typeof(IRowGuidEnabled).IsAssignableFrom(typeof(TestUser));
        }

        public async Task AddAsync(TestUser entity)
        {
            await _context.AddAsync(entity);
        }

#pragma warning disable 1998
        public async Task UpdateAsync(TestUser entity)
#pragma warning restore 1998
        {
            _context.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

#pragma warning disable 1998
        public async Task DeleteAsync(TestUser entity)
#pragma warning restore 1998
        {
            _context.Users.Remove(entity);
        }

#pragma warning disable 1998
        public async Task DeleteAsync(Expression<Func<TestUser, bool>> where)
#pragma warning restore 1998
        {
            var objects = _dbset.Where(where).AsEnumerable();
            foreach (var obj in objects)
                _dbset.Remove(obj);
        }

        public async Task<TestUser> GetByIdAsync(int id)
        {
            return await _dbset.FindAsync(id);
        }
        
        public async Task<TestUser> GetAsync(Expression<Func<TestUser, bool>> @where)
        {
            return await _dbset.FirstOrDefaultAsync(where);
        }

        public IQueryable<TestUser> GetAll()
        {
            return _dbset.AsQueryable();
        }

        public async Task<TestUser[]> GetByIdsAsync(int[] ids)
        {
            return await _dbset.ToArrayAsync();
        }

        public async Task AddAsync(object entity)
        {
            await AddAsync((TestUser)entity);
        }

        public async Task UpdateAsync(object entity)
        {
            await UpdateAsync((TestUser)entity);
        }

        public async Task DeleteAsync(object entity)
        {
            await DeleteAsync((TestUser)entity);
        }

        public async Task<object> GetByRowGuid(Guid rowGuid)
        {
            if (_isRowGuidSupported)
                throw new Exception("RowGuid is not supported");

            return await GetAsync(arg => ((IRowGuidEnabled) arg).RowGuid == rowGuid);
        }
    }
}
