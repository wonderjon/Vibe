using System.Linq.Expressions;

namespace VibeCheck.DataAcces.Repositories;

public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Raw queryable for complex reads (filtering, paging, includes) built by the caller.
    /// No tracking by default — callers opt into tracking only when they intend to mutate.
    /// </summary>
    IQueryable<T> Query(bool tracked = false);

    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);

    void RemoveRange(IEnumerable<T> entities);
}
