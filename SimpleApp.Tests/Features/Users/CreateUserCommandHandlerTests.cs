using FluentAssertions;
using MassTransit;
using NSubstitute;
using SimpleApp.Api.Features.Users.Create;
using SimpleApp.Domain.Entities.Users;
using SimpleApp.Domain.Events;
using SimpleApp.Tests.Common;

namespace SimpleApp.Tests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();

    private CreateUserCommandHandler CreateHandler(out SimpleApp.Persistence.Contexts.DataContext db)
    {
        db = TestDbContextFactory.Create();
        return new CreateUserCommandHandler(db, _publishEndpoint);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessWithUserId()
    {
        // Arrange
        var handler = CreateHandler(out _);
        var command = BuildCommand("John", "Doe", "john@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidRequest_SavesUserToDatabase()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        var command = BuildCommand("John", "Doe", "john@example.com");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        db.Users.Should().HaveCount(1);
        db.Users.First().Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_ValidRequest_PublishesUserCreatedEvent()
    {
        // Arrange
        var handler = CreateHandler(out _);
        var command = BuildCommand("John", "Doe", "john@example.com");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _publishEndpoint.Received(1)
            .Publish(Arg.Any<UserCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        db.Users.Add(User.Create("Jane", "Doe", "existing@example.com", null, true));
        await db.SaveChangesAsync();

        var command = BuildCommand("John", "Doe", "existing@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("already exists"));
    }

    [Fact]
    public async Task Handle_DuplicateEmail_DoesNotPublishEvent()
    {
        // Arrange
        var handler = CreateHandler(out var db);
        db.Users.Add(User.Create("Jane", "Doe", "existing@example.com", null, true));
        await db.SaveChangesAsync();

        var command = BuildCommand("John", "Doe", "existing@example.com");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _publishEndpoint.DidNotReceive()
            .Publish(Arg.Any<UserCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    private static CreateUserCommand BuildCommand(
        string firstName, string lastName, string email, string? phone = null, bool isActive = true)
        => new()
        {
            Request = new CreateUserRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                IsActive = isActive
            }
        };
}
