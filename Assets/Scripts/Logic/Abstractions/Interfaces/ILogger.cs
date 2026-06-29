#nullable enable

public interface ILogger
{
    void Log(string message, LogLevel level = LogLevel.Info);
    void LogError(string error);
    void LogWarning(string warning);
    void LogDebug(string debug);
}