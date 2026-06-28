#nullable enable


public sealed record BoardConfig
{
    public bool Is3D { get; init; } = false;
    public int FieldSize { get; init; } = 8;
    public int FrameThickness { get; init; } = 0;
    public int NumberOfSides { get; init; } = 2;
    public int NumberOfPlayersOnSide { get; init; } = 1;
}