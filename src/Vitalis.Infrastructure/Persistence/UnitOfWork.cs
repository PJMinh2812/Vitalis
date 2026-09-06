using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities;
using Vitalis.Infrastructure.Persistence.Repositories;

namespace Vitalis.Infrastructure.Persistence;

public class UnitOfWork(VitalisDbContext context) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = [];

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        if (_repositories.TryGetValue(typeof(T), out var existing))
            return (IRepository<T>)existing;

        var repository = new Repository<T>(context);
        _repositories[typeof(T)] = repository;
        return repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
