using FluentResults;
using MediatR;
using SimpleApp.Shared.Caching;
using SimpleApp.Shared.Messaging;

namespace SimpleApp.Api.Base.Behaviors;

public class QueryCachingPipelineBehavior<TRequest, TResponse>(IRedisCacheService cacheService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await cacheService.GetOrCreateAsync(
            request.CacheKey,
            _ => next(),
            cancellationToken);
    }
}