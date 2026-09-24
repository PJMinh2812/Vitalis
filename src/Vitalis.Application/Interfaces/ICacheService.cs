namespace Vitalis.Application.Interfaces;

// Thin cache-aside abstraction — Application knows nothing about Redis or
// IDistributedCache, same pattern as IPasswordHasher/IJwtTokenGenerator.
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
