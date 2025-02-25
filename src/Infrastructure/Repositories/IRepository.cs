using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public interface IRepository<T>
    {
        Task<int> CountAsync(Expression<Func<T, bool>> expression);
        Task CreateAsync(T entity);
        Task DeleteAsync(T entity);
        void Dispose();
        Task<T?> GetAsync(dynamic id);
        IQueryable<T> List();
        IQueryable<T> ListAsNoTracking();
        Task SaveChangesAsync();
        Task UpdateAsync(T entity);
    }
}