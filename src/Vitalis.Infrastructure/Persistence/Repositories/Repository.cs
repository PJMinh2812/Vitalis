using Microsoft.EntityFrameworkCore;
using Vitalis.Application.Common;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities;

namespace Vitalis.Infrastructure.Persistence.Repositories;

public class Repository<T>(VitalisDbContext context) : IRepository<T> where T : BaseEntity
{
    private readonly DbSet<T> _set = context.Set<T>();

    public IQueryable<T> Query() => _set.AsQueryable();

    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _set.FindAsync([id], cancellationToken).AsTask();

    public Task<T?> FirstOrDefaultAsync(IQueryable<T> query, CancellationToken cancellationToken = default) =>
        query.FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<T>> GetPagedAsync(IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, page, pageSize);
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        _set.AddAsync(entity, cancellationToken).AsTask();

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}
