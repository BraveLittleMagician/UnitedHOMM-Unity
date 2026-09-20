#nullable enable

public class LoggerNull : ILogger
{
    public void Log(string message, LogLevel level = LogLevel.Info) { }
    public void LogError(string error) { }
    public void LogWarning(string warning) { }
    public void LogDebug(string debug) { }
}