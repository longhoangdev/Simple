using FluentResults;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace SimpleApp.Shared.Caching;

public class RedisCacheService(IDistributedCache distributedCache) : IRedisCacheService
{
    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        var value = await distributedCache.GetStringAsync(key, cancellationToken);

        if (value is not null) return (T)Convert.ChangeType(value, typeof(T));
        
        var result = await factory(cancellationToken);
        
        await distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(result), token: cancellationToken);
        return result;
    }
}