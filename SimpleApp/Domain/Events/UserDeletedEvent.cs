namespace SimpleApp.Domain.Events;

public record UserDeletedEvent(
    Guid UserId,
    DateTimeOffset OccurredAt);
