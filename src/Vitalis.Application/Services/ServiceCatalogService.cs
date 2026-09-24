using Vitalis.Application.DTOs.Billing;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Billing;

namespace Vitalis.Application.Services;

public class ServiceCatalogService(IUnitOfWork unitOfWork, ICacheService cache) : IServiceCatalogService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private static string CacheKey(bool? activeOnly) => $"services:{activeOnly}";

    public async Task<IReadOnlyList<ServiceDto>> GetAllAsync(bool? activeOnly, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKey(activeOnly);
        var cached = await cache.GetAsync<List<ServiceDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var repository = unitOfWork.Repository<Service>();
        var query = repository.Query();

        if (activeOnly == true)
            query = query.Where(s => s.IsActive);

        query = query.OrderBy(s => s.Name);
        var items = await repository.ToListAsync(query, cancellationToken);
        var result = items.Select(s => s.ToDto()).ToList();

        await cache.SetAsync(cacheKey, result, CacheDuration, cancellationToken);
        return result;
    }

    public async Task<ServiceDto> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Service>();

        if (request.Code is not null)
        {
            var codeTaken = await repository.FirstOrDefaultAsync(
                repository.Query().Where(s => s.Code == request.Code), cancellationToken) is not null;
            if (codeTaken)
                throw new ConflictException($"Mã dịch vụ '{request.Code}' đã tồn tại");
        }

        var service = new Service
        {
            Code = request.Code,
            SpecialtyId = request.SpecialtyId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
        };
        await repository.AddAsync(service, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await InvalidateCacheAsync(cancellationToken);
        return service.ToDto();
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Service>();
        var service = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Service), id);

        service.IsActive = false;
        service.UpdatedAt = DateTime.UtcNow;
        repository.Update(service);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await InvalidateCacheAsync(cancellationToken);
    }

    private async Task InvalidateCacheAsync(CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CacheKey(true), cancellationToken);
        await cache.RemoveAsync(CacheKey(null), cancellationToken);
    }
}
