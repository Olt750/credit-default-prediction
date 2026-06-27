using CreditDefault.Api.Data;
using CreditDefault.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext Context;
        protected readonly DbSet<TEntity> Set;

        public Repository(AppDbContext context)
        {
            Context = context;
            Set = context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(Guid id) => await Set.FindAsync(id);
        public async Task<List<TEntity>> GetAllAsync() => await Set.ToListAsync();
        public async Task AddAsync(TEntity entity) { Set.Add(entity); await Context.SaveChangesAsync(); }
        public async Task UpdateAsync(TEntity entity) { Set.Update(entity); await Context.SaveChangesAsync(); }
        public async Task DeleteAsync(TEntity entity) { Set.Remove(entity); await Context.SaveChangesAsync(); }
    }
}
