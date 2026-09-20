#nullable enable

using System.Collections.Generic;

public static class StartingPlacementPreset
{
    public static IReadOnlyList<StartingPlacement> CreateStandard()
    {
        return new List<StartingPlacement>
        {
            new(new PieceDefinition("Pawn", new IndexOfPlayer(0, 0), 10), new Square{ X = 2, Y = 2 }),
            new(new PieceDefinition("Pawn", new IndexOfPlayer(0, 0), 10), new Square{ X = 3, Y = 2 }),
        };
    }
}