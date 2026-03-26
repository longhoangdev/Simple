namespace SimpleApp.Domain.Events;

public record UserUpdatedEvent(
    Guid UserId,
    string Email,
    DateTimeOffset OccurredAt);
