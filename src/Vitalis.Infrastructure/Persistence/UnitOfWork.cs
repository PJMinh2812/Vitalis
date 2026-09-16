using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vitalis.Application.Exceptions;
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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // A unique index (e.g. UX_appointments_doctor_slot) rejected the write —
            // this is the real double-booking backstop. Translate it into the same
            // Application-layer exception a service-level pre-check would throw, so
            // callers (AppointmentService) only ever have to handle one type.
            throw new ConflictException("Dữ liệu vừa xung đột với một bản ghi khác, vui lòng thử lại");
        }
    }
}
