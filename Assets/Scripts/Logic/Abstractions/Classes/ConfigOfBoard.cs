#nullable enable

public sealed record ConfigOfBoard
{
    public MultipleAxesFromTwo Axes { get; init; } = MultipleAxesFromTwo.Two;
    public int FieldSize { get; init; } = 8; 
    public bool WUp { get; init; } = false;
    public bool WDown { get; init; } = false;
    public int NumberOfSides { get; init; } = 2;
    public int NumberOfPlayersOnSide { get; init; } = 1;
}
