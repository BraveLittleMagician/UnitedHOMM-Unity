#nullable enable

public sealed record BoardConfigChangedEvent(ConfigOfBoard OldConfig, ConfigOfBoard NewConfig);