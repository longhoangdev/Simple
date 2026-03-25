using Microsoft.Extensions.Caching.Memory;

namespace SimpleApp.Shared.Caching;

internal sealed class CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger) : ICacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, bool useSlidingExpiration = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        var value = await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            if (useSlidingExpiration)
                entry.SetSlidingExpiration(expiration ?? DefaultExpiration);
            else
                entry.SetAbsoluteExpiration(expiration ?? DefaultExpiration);

            return await factory(cancellationToken);


        });
        return value;
    }
}