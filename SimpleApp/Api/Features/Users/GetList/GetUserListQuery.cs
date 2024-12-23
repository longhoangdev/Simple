using SimpleApp.Shared.Messaging;

namespace SimpleApp.Api.Features.Users.GetList;

internal sealed class GetUserListQuery : ICachedQuery<GetUserListResponse>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public string CacheKey => $"get-user-list-{Page}-{PageSize}";
    public TimeSpan? Expiration => null;
}