using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public abstract class Repository<T> : IRepository<T> where T : class
{
    private bool _disposedValue;

    protected readonly DbSet<T> Set;
    protected readonly DbContext Context;

    protected Repository(Context context)
    {
        Context = context;
        Set = Context.Set<T>();
    }

    public async Task<T?> GetAsync(dynamic id)
    {
        return await Set.FindAsync(id);
    }

    public IQueryable<T> List()
    {
        return Set;
    }

    public IQueryable<T> ListAsNoTracking()
    {
        return Set.AsNoTracking();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
    {
        return await Set.CountAsync(expression);
    }

    public Task CreateAsync(T entity)
    {
        Set.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        Set.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        Set.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;

        if (disposing)
        {
            Context?.Dispose();
        }

        _disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}


