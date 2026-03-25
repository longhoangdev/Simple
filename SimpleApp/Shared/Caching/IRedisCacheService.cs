namespace SimpleApp.Shared.Caching;

public interface IRedisCacheService
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default);
}