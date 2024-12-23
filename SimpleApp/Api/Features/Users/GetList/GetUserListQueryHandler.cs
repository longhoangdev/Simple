using FluentResults;
using Microsoft.EntityFrameworkCore;
using SimpleApp.Persistence.Contexts;
using SimpleApp.Shared.Messaging;

namespace SimpleApp.Api.Features.Users.GetList;

internal sealed class GetUserListQueryHandler(ReadOnlyDataContext context) : IQueryHandler<GetUserListQuery, GetUserListResponse>
{
    public async Task<Result<GetUserListResponse>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        var userQuery = context.Users;
        var total = await userQuery.CountAsync(cancellationToken);
        var users = await userQuery.OrderBy(r => r.FirstName).Skip(request.Page * request.PageSize).Select(u => new GetUserListResponseItem
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            Phone = u.Phone,
            IsActive = u.IsActive
        }).ToListAsync(cancellationToken);  

        return Result.Ok(new GetUserListResponse
        {
            Total = total,
            Items = users
        });
    }
}