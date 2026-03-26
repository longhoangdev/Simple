using MassTransit;
using SimpleApp.Domain.Events;
using SimpleApp.Shared.Logging;

namespace SimpleApp.Api.Features.Users;

// Single consumer that handles all User domain events.
// Idempotency is handled automatically by MassTransit InboxState table.
public sealed class UserEventConsumer(IDataDogLogService dataDog) :
    IConsumer<UserCreatedEvent>,
    IConsumer<UserUpdatedEvent>,
    IConsumer<UserDeletedEvent>
{
    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var evt = context.Message;
        return dataDog.LogInfoAsync(
            $"Welcome email stub: sending welcome to {evt.Email} for user {evt.UserId}",
            new Dictionary<string, object>
            {
                ["userId"] = evt.UserId,
                ["email"] = evt.Email,
                ["firstName"] = evt.FirstName,
                ["lastName"] = evt.LastName,
                ["occurredAt"] = evt.OccurredAt,
                ["event"] = "UserCreated"
            },
            context.CancellationToken);
    }

    public Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var evt = context.Message;
        return dataDog.LogInfoAsync(
            $"Audit: user {evt.UserId} profile updated",
            new Dictionary<string, object>
            {
                ["userId"] = evt.UserId,
                ["email"] = evt.Email,
                ["occurredAt"] = evt.OccurredAt,
                ["event"] = "UserUpdated"
            },
            context.CancellationToken);
    }

    public Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var evt = context.Message;
        return dataDog.LogInfoAsync(
            $"Audit: user {evt.UserId} was soft-deleted",
            new Dictionary<string, object>
            {
                ["userId"] = evt.UserId,
                ["occurredAt"] = evt.OccurredAt,
                ["event"] = "UserDeleted"
            },
            context.CancellationToken);
    }
}

public sealed class UserEventConsumerDefinition : ConsumerDefinition<UserEventConsumer>
{
    public UserEventConsumerDefinition()
    {
        EndpointName = "user-events";
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<UserEventConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r =>
            r.Intervals(
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5)));
    }
}
