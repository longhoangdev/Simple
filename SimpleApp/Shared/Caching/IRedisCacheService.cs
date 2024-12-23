using FluentResults;

namespace SimpleApp.Shared.Caching;

public interface IRedisCacheService
{
    Task<Result<T>> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<Result<T>>> factory,
        TimeSpan? expiration = null,
        bool useSlidingExpiration = false,
        CancellationToken cancellationToken = default);
}