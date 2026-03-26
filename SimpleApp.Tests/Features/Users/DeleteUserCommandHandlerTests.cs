using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SimpleApp.Api.Features.Users.Delete;
using SimpleApp.Domain.Entities.Users;
using SimpleApp.Domain.Events;
using SimpleApp.Tests.Common;

namespace SimpleApp.Tests.Features.Users;

public class DeleteUserCommandHandlerTests
{
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();

    private DeleteUserCommandHandler CreateHandler(out SimpleApp.Persistence.Contexts.DataContext db)
    {
        db = TestDbContextFactory.Create();
        return new DeleteUserCommandHandler(db, _publishEndpoint);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsSuccess()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        var user = User.Create("John", "Doe", "john@example.com", null, true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var command = new DeleteUserCommand { UserId = user.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ExistingUser_SoftDeletesUser()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        var user = User.Create("John", "Doe", "john@example.com", null, true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var command = new DeleteUserCommand { UserId = user.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — reload from DB bypassing the soft-delete query filter
        db.ChangeTracker.Clear();
        var deleted = db.Users.IgnoreQueryFilters().First(u => u.Id == user.Id);
        deleted.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExistingUser_PublishesUserDeletedEvent()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        var user = User.Create("John", "Doe", "john@example.com", null, true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var command = new DeleteUserCommand { UserId = user.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _publishEndpoint.Received(1)
            .Publish(Arg.Is<UserDeletedEvent>(e => e.UserId == user.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingUser_PublishesEventWithCorrectUserId()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        var user = User.Create("John", "Doe", "john@example.com", null, true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var command = new DeleteUserCommand { UserId = user.Id };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _publishEndpoint.Received(1)
            .Publish(Arg.Is<UserDeletedEvent>(e => e.UserId == user.Id), Arg.Any<CancellationToken>());
    }
}
