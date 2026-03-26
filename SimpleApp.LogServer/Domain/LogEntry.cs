using System.ComponentModel.DataAnnotations;

namespace SimpleApp.LogServer.Domain;

public sealed class LogEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Service { get; init; }
    public required string Level { get; init; }
    [MaxLength(4000)]
    public required string Message { get; init; }
    public string? Source { get; init; }
    public string? Hostname { get; init; }
    public string? Tags { get; init; }
    public Dictionary<string, object>? Attributes { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ReceivedAt { get; init; } = DateTimeOffset.UtcNow;
}
