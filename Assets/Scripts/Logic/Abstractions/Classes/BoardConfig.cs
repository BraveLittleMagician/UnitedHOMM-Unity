#nullable enable

public sealed record BoardConfig
{
    public MultipleAxesFromTwo Axes { get; init; } = MultipleAxesFromTwo.Two;
    public int FieldSize { get; init; } = 8;
    public int NumberOfSides { get; init; } = 2;
    public int NumberOfPlayersOnSide { get; init; } = 1;
}
