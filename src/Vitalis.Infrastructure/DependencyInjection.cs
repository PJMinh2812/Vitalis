using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Vitalis.Application.Common;
using Vitalis.Application.Interfaces;
using Vitalis.Infrastructure.Caching;
using Vitalis.Infrastructure.Persistence;
using Vitalis.Infrastructure.Persistence.Repositories;
using Vitalis.Infrastructure.Security;

namespace Vitalis.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<VitalisDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Plain singleton instead of IOptions<T> — the secret never changes at
        // runtime, so the options-reload machinery would be unused weight.
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");
        services.AddSingleton(jwtSettings);

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Missing 'Redis' connection string.");
        // AbortOnConnectFail = false so a Redis outage at startup doesn't crash
        // every request that resolves IConnectionMultiplexer (e.g. /health) —
        // it keeps retrying in the background instead; RedisHealthCheck/
        // RedisCacheService already handle a still-unreachable Redis gracefully.
        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.AbortOnConnectFail = false;
        // Registered separately (not just via AddStackExchangeRedisCache) so
        // RedisHealthCheck can ping the same connection directly.
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
