using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using EFCore.BulkExtensions;

namespace SigmaProConnectFileValidation.Data
{
    [ExcludeFromCodeCoverage]
    public class DataAccessProvider<T> : IDataAccessProvider<T> where T : class
    {
        private readonly SigmaproConnectContext dbContext;

        public DataAccessProvider(SigmaproConnectContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IQueryable<T> GetAll(bool isRO = false)
        {
            return isRO ? dbContext.Set<T>().AsNoTracking() : dbContext.Set<T>().AsNoTracking();
        }

        public async Task<List<T>> GetAllByExpression(Expression<Func<T, bool>> expression, bool isRO)
        {
            return isRO
                ? await dbContext.Set<T>().Where(expression).ToListAsync()
                : await dbContext.Set<T>().Where(expression).ToListAsync();
        }

        public async Task<T?> GetByExpression(Expression<Func<T, bool>> expression, bool isRO)
        {
            return isRO
                ? await dbContext.Set<T>().Where(expression).FirstOrDefaultAsync()
                : await dbContext.Set<T>().Where(expression).FirstOrDefaultAsync();
        }

        public async Task<T?> GetById(int id, bool isRO)
        {
            return isRO ? await dbContext.Set<T>().FindAsync(id) : await dbContext.Set<T>().FindAsync(id);
        }

        public async Task Create(T entity)
        {
            await dbContext.Set<T>().AddAsync(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task<T> CreateIdentity(T entity)
        {
            await dbContext.Set<T>().AddAsync(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task CreateMany(IEnumerable<T> entities)
        {
            await dbContext.BulkInsertAsync(entities.ToList());
        }

        public async Task Update(int id, T entity)
        {
            dbContext.Set<T>().Update(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task Update(long id, T entity)
        {
            dbContext.Set<T>().Update(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task Update(string id, T entity)
        {
            dbContext.Set<T>().Update(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateRequiredProperties(long id, List<string> propertyList, T entity)
        {
            foreach (var item in propertyList)
                dbContext.Entry(entity).Property(item).IsModified = true;

            dbContext.Set<T>().Update(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateRequiredProperties(int id, List<string> propertyList, T entity)
        {
            foreach (var item in propertyList)
                dbContext.Entry(entity).Property(item).IsModified = true;

            dbContext.Set<T>().Update(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateRequiredProperties(string id, List<string> propertyList, T entity)
        {
            foreach (var item in propertyList)
                dbContext.Entry(entity).Property(item).IsModified = true;

            dbContext.Set<T>().Update(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateMany(IEnumerable<T> entities)
        {
            dbContext.Set<T>().UpdateRange(entities);
            await dbContext.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var entity = await GetById(id, false);
            dbContext.Set<T>().Remove(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteMany(IEnumerable<T> entities)
        {
            await dbContext.BulkDeleteAsync<T>(entities.ToList());
        }
    }
}
