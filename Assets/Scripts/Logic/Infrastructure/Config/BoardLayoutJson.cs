#nullable enable

public sealed record BoardLayoutJson(BoardSizeJson? GridSize, PlacementJson[]? Placements);
public sealed record BoardSizeJson(int X, int Y, int Z, string? WUp, string? WDown);
public sealed record PlacementJson(PositionJson? Position, string? Name, PlayerJson? Player);
public sealed record PlayerJson(int IndexOfSide, int IndexOfPlayerOnSide);
public sealed record PositionJson(int X, int Y, int Z, int W);