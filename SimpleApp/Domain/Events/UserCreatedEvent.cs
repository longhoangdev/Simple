namespace SimpleApp.Domain.Events;

public record UserCreatedEvent(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    DateTimeOffset OccurredAt);
