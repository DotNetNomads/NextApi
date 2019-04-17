using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.Tests.EntityService.DAL
{
    public class TestUserRepository : INextApiRepository<TestUser, int>
    {
        private readonly TestDbContext _context;
        private readonly DbSet<TestUser> _dbset;

        public TestUserRepository(TestDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbset = _context.Users;
        }

        public async Task AddAsync(TestUser entity)
        {
            await _context.AddAsync(entity);
        }

        public async Task UpdateAsync(TestUser entity)
        {
            _context.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task DeleteAsync(TestUser entity)
        {
            _context.Users.Remove(entity);
        }

        public async Task DeleteAsync(Expression<Func<TestUser, bool>> where)
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

        public Expression<Func<TestUser, bool>> KeyPredicate(int key)
        {
            return e => e.Id == key;
        }

        /// <inheritdoc />
        public Expression<Func<TestUser, bool>> KeyPredicate(int[] keys)
        {
            return entity => keys.Contains(entity.Id);
        }

        public async Task<TestUser[]> GetByIdsAsync(int[] ids)
        {
            return await _dbset.ToArrayAsync();
        }
    }
}
