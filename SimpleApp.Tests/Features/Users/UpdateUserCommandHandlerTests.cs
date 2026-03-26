using FluentAssertions;
using MassTransit;
using NSubstitute;
using SimpleApp.Api.Features.Users.Update;
using SimpleApp.Domain.Entities.Users;
using SimpleApp.Domain.Events;
using SimpleApp.Tests.Common;

namespace SimpleApp.Tests.Features.Users;

public class UpdateUserCommandHandlerTests
{
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();

    private UpdateUserCommandHandler CreateHandler(out SimpleApp.Persistence.Contexts.DataContext db)
    {
        db = TestDbContextFactory.Create();
        return new UpdateUserCommandHandler(db, _publishEndpoint);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        var user = User.Create("John", "Doe", "john@example.com", null, true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var command = BuildCommand(user.Id, "Jane", "Smith", "jane@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesUserInDatabase()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        var user = User.Create("John", "Doe", "john@example.com", null, true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var command = BuildCommand(user.Id, "Jane", "Smith", "jane@example.com");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = db.Users.First();
        updated.FirstName.Should().Be("Jane");
        updated.LastName.Should().Be("Smith");
        updated.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task Handle_ValidRequest_PublishesUserUpdatedEvent()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        var user = User.Create("John", "Doe", "john@example.com", null, true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var command = BuildCommand(user.Id, "Jane", "Smith", "jane@example.com");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _publishEndpoint.Received(1)
            .Publish(Arg.Any<UserUpdatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailAlreadyTakenByAnotherUser_ReturnsFailure()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        var user1 = User.Create("John", "Doe", "john@example.com", null, true);
        var user2 = User.Create("Jane", "Doe", "jane@example.com", null, true);
        db.Users.AddRange(user1, user2);
        await db.SaveChangesAsync();

        // Try to update user1's email to user2's email
        var command = BuildCommand(user1.Id, "John", "Doe", "jane@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("already exists"));
    }

    [Fact]
    public async Task Handle_SameEmail_DoesNotCheckForDuplicate_ReturnsSuccess()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        var user = User.Create("John", "Doe", "john@example.com", null, true);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Same email — should be allowed
        var command = BuildCommand(user.Id, "Johnny", "Doe", "john@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    private static UpdateUserCommand BuildCommand(
        Guid userId, string firstName, string lastName, string email, string? phone = null, bool isActive = true)
        => new()
        {
            UserId = userId,
            Request = new UpdateUserRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                IsActive = isActive
            }
        };
}
