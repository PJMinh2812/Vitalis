using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Domain.Entities.Scheduling;

namespace Vitalis.Application.Services;

public class SpecialtyService(IUnitOfWork unitOfWork, ICacheService cache) : ISpecialtyService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    // Read constantly (every booking dropdown, VC-03) and changes rarely (an
    // admin adding a specialty) — a textbook cache-aside candidate, unlike
    // doctor available-slots which change on every booking.
    private static string CacheKey(bool? activeOnly) => $"specialties:{activeOnly}";

    public async Task<IReadOnlyList<SpecialtyDto>> GetAllAsync(bool? activeOnly, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKey(activeOnly);
        var cached = await cache.GetAsync<List<SpecialtyDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var repository = unitOfWork.Repository<Specialty>();
        var query = repository.Query();

        if (activeOnly == true)
            query = query.Where(s => s.IsActive);

        query = query.OrderBy(s => s.Name);

        var items = await repository.ToListAsync(query, cancellationToken);
        var result = items.Select(s => s.ToDto()).ToList();

        await cache.SetAsync(cacheKey, result, CacheDuration, cancellationToken);
        return result;
    }

    public async Task<SpecialtyDto> CreateAsync(CreateSpecialtyRequest request, CancellationToken cancellationToken = default)
    {
        var repository = unitOfWork.Repository<Specialty>();

        if (request.Code is not null)
        {
            var codeTaken = await repository.FirstOrDefaultAsync(
                repository.Query().Where(s => s.Code == request.Code), cancellationToken) is not null;
            if (codeTaken)
                throw new ConflictException($"Mã chuyên khoa '{request.Code}' đã tồn tại");
        }

        var specialty = new Specialty
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
        };
        await repository.AddAsync(specialty, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Only two cache keys ever exist (activeOnly true/null) — invalidate both
        // rather than trying to patch the cached lists in place.
        await cache.RemoveAsync(CacheKey(true), cancellationToken);
        await cache.RemoveAsync(CacheKey(null), cancellationToken);

        return specialty.ToDto();
    }
}
