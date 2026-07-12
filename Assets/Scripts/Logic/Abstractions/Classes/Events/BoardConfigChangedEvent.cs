#nullable enable

public record BoardConfigChangedEvent(BoardConfig OldConfig, BoardConfig NewConfig);