using FluentResults;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace SimpleApp.Shared.Caching;

public class RedisCacheService(IDistributedCache distributedCache) : IRedisCacheService
{
    public async Task<Result<T>> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<Result<T>>> factory, TimeSpan? expiration = null, bool useSlidingExpiration = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        var value = await distributedCache.GetStringAsync(key, cancellationToken);

        if (value is not null) return (T)Convert.ChangeType(value, typeof(T));
        
        var result = await factory(cancellationToken);
        
        if (!result.IsSuccess) return Result.Fail<T>("Value not found");
        
        await distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(result.Value), token: cancellationToken);
        return result.Value;

    }
}