#nullable enable

public sealed record BoardConfigChangedEvent(BoardConfig OldConfig, BoardConfig NewConfig);