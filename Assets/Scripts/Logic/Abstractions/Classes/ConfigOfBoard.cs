#nullable enable

public sealed record ConfigOfBoard
{
    public bool? SignWNegative { get; init; }
    public int FieldSize { get; init; } = 8;
    public int W { get; init; } = 1;
    public int NumberOfSides { get; init; } = 2;
    public int NumberOfPlayersOnSide { get; init; } = 1;
    public int WWOffset => W switch
    {
        1 => 0,
        2 => SignWNegative == true ? -1 : 0,
        3 => -1,
        _ => 0,
    };
    public MultipleAxesFromTwo Axes { get; init; } = MultipleAxesFromTwo.Two;
}