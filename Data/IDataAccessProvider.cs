using System.Linq.Expressions;

namespace SigmaProConnectFileValidation.Data
{
    public interface IDataAccessProvider<T> where T : class
    {
        IQueryable<T> GetAll(bool isRO = false);

        Task<List<T>> GetAllByExpression(Expression<Func<T, bool>> expression, bool isRO);

        Task<T> GetByExpression(Expression<Func<T, bool>> expression, bool isRO);

        Task<T> GetById(int id, bool isRO);

        Task Create(T entity);

        Task<T> CreateIdentity(T entity);

        Task CreateMany(IEnumerable<T> entities);

        Task Update(int id, T entity);

        Task Update(long id, T entity);

        Task Update(string id, T entity);

        Task UpdateRequiredProperties(long id, List<string> propertyList, T entity);

        Task UpdateRequiredProperties(int id, List<string> propertyList, T entity);

        Task UpdateRequiredProperties(string id, List<string> propertyList, T entity);

        Task UpdateMany(IEnumerable<T> entities);

        Task Delete(int id);

        Task DeleteMany(IEnumerable<T> entities);
    }
}
