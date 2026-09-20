#nullable enable

using UnityEngine;

public class LoggerForUnity : ILogger
{
    public void Log(string message, LogLevel level = LogLevel.Info) => Debug.Log($"[{level}] {message}");
    public void LogError(string error) => Debug.LogError($"[Error] {error}");
    public void LogWarning(string warning) => Debug.LogWarning($"[Warning] {warning}");
    public void LogDebug(string debug) => Debug.Log($"[Debug] {debug}");
}