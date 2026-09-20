#nullable enable

using System;

public class LoggerForConsole : ILogger
{
    public void Log(string message, LogLevel level = LogLevel.Info)
    {
        var color = level switch
        {
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
        Console.ForegroundColor = color;
        Console.WriteLine($"[{level}] {DateTime.Now:HH:mm:ss} - {message}");
        Console.ResetColor();
    }

    public void LogError(string error) => Log(error, LogLevel.Error);
    public void LogWarning(string warning) => Log(warning, LogLevel.Warning);
    public void LogDebug(string debug) => Log(debug, LogLevel.Debug);
}