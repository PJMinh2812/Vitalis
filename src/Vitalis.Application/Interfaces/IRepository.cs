using Vitalis.Application.Common;
using Vitalis.Domain.Entities;

namespace Vitalis.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    // Exposed so a Service can compose filters (Where/OrderBy) before paging or projecting.
    IQueryable<T> Query();

    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<T>> GetPagedAsync(IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);
}
