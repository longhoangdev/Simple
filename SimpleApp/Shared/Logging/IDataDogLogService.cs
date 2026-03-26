namespace SimpleApp.Shared.Logging;

public interface IDataDogLogService
{
    Task LogAsync(string message, string level = "INFO", Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default);
    Task LogInfoAsync(string message, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default);
    Task LogWarnAsync(string message, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default);
    Task LogErrorAsync(string message, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default);
}
