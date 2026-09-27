#nullable enable

using System.Collections.Generic;

public static class StartingPlacementPreset
{
    public static IReadOnlyList<StartingPlacement> CreateStandard(DeckSequenceProvider deckSequences)
    {
        var p0 = new IndexOfPlayer(0, 0);
        var p1 = new IndexOfPlayer(1, 0);

        var deckP0 = deckSequences.GetFor(p0);
        var deckP1 = deckSequences.GetFor(p1);

        var pawnBoardMovement = new MovementCombinedFactory<Pattern<Square>, Board, Square>(new Pattern<Square>());
        var pawnDeckMovementP0 = new MovementCombinedFactory<SizeAwareInt, Decks, int>(deckP0);
        var pawnDeckMovementP1 = new MovementCombinedFactory<SizeAwareInt, Decks, int>(deckP1);

        return new List<StartingPlacement>
        {
            new(new PieceDefinition("Pawn", p0, 10,
                MovementFactories: new IMovementFactory[] { pawnBoardMovement, pawnDeckMovementP0 }),
                new Square { X = 2, Y = 2 }),

            new(new PieceDefinition("Pawn", p0, 10,
                MovementFactories: new IMovementFactory[] { pawnBoardMovement, pawnDeckMovementP0 }),
                new Square { X = 3, Y = 2 }),

            new(new PieceDefinition("Pawn", p1, 10,
                MovementFactories: new IMovementFactory[] { pawnBoardMovement, pawnDeckMovementP1 }),
                new Square { X = 5, Y = 5 }),
        };
    }
}