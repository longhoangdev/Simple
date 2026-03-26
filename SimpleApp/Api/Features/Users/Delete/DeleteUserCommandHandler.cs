﻿using FluentResults;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SimpleApp.Domain.Events;
using SimpleApp.Persistence.Contexts;
using SimpleApp.Shared.Messaging;

namespace SimpleApp.Api.Features.Users.Delete;

internal sealed class DeleteUserCommandHandler(DataContext context, IPublishEndpoint publishEndpoint)
    : ICommandHandler<DeleteUserCommand, DeleteUserResponse>
{
    public async Task<Result<DeleteUserResponse>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstAsync(u => u.Id == request.UserId, cancellationToken);
        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(
            new UserDeletedEvent(user.Id, DateTimeOffset.UtcNow),
            cancellationToken);

        return Result.Ok(new DeleteUserResponse
        {
            UserId = request.UserId
        });
    }
}