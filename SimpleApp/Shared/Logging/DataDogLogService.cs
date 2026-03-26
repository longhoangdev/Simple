using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SimpleApp.Shared.Logging;

public sealed class DataDogLogService(
    HttpClient httpClient,
    ILogger<DataDogLogService> logger) : IDataDogLogService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task LogAsync(
        string message,
        string level = "INFO",
        Dictionary<string, object>? attributes = null,
        CancellationToken cancellationToken = default)
    {
        var entry = new DataDogLogEntry
        {
            Message = message,
            Level = level,
            Attributes = attributes
        };

        try
        {
            var json = JsonSerializer.Serialize(entry, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));

            var response = await httpClient.PostAsync(string.Empty, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Fallback to local logger if DataDog is unavailable — never lose a log
                logger.LogWarning(
                    "DataDog log delivery failed with {StatusCode}. Message: {Message}",
                    response.StatusCode, message);
            }
        }
        catch (Exception ex)
        {
            // Never throw from a logging service — fallback silently to local logger
            logger.LogWarning(ex, "DataDog log delivery threw an exception. Message: {Message}", message);
        }
    }

    public Task LogInfoAsync(string message, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default)
        => LogAsync(message, "INFO", attributes, cancellationToken);

    public Task LogWarnAsync(string message, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default)
        => LogAsync(message, "WARN", attributes, cancellationToken);

    public Task LogErrorAsync(string message, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default)
        => LogAsync(message, "ERROR", attributes, cancellationToken);
}
