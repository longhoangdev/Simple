using System.Text.Json.Serialization;

namespace SimpleApp.Shared.Logging;

public sealed class DataDogLogEntry
{
    [JsonPropertyName("ddsource")]
    public string Source { get; init; } = "simpleapp";

    [JsonPropertyName("service")]
    public string Service { get; init; } = "simpleapp";

    [JsonPropertyName("hostname")]
    public string Hostname { get; init; } = Environment.MachineName;

    [JsonPropertyName("ddtags")]
    public string Tags { get; init; } = string.Empty;

    [JsonPropertyName("level")]
    public string Level { get; init; } = "INFO";

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("attributes")]
    public Dictionary<string, object>? Attributes { get; init; }
}
