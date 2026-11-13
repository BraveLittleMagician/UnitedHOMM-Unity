#nullable enable

using System;
using System.Collections.Generic;

public static class StandardPieceTemplates
{
    public static IReadOnlyList<PieceTemplate> CreateAll(DeckSequenceProvider deckSequences, IEventBus eventBus)
    {
        var noAbilities = Array.Empty<IAbility>();

        var kingMovement = new IMovementFactory[]
        {
            new MovementFactory<RelativeArea<Square>>(new RelativeArea<Square>(MultipleAxes.Two, isCircle: false))
        };

        var pawnMovement = new IMovementFactory[]
        {
            new MovementFactory<Pattern<Square>>(PawnRules.CreateMoveShape(), PawnRules.CreateAttackShape())
        };

        var rookMovement = new IMovementFactory[]
        {
            new MovementFactory<PathSetPattern>(new PathSetPattern(RookRules.Create(), maxRepeat: 8))
        };

        var bishopMovement = new IMovementFactory[]
        {
            new MovementFactory<PathSetPattern>(new PathSetPattern(BishopRules.Create(), maxRepeat: 8))
        };

        var queenMovement = new IMovementFactory[]
        {
            new MovementFactory<PathSetPattern>(new PathSetPattern(QueenRules.Create(), maxRepeat: 8))
        };

        var knightMovement = new IMovementFactory[]
        {
            new MovementFactory<PathSetPattern>(new PathSetPattern(KnightRules.Create(), maxRepeat: 8))
        };

        return new List<PieceTemplate>
        {
            new("Pawn",   10, 1, 1, FacingDirection.PlusY, pawnMovement,   noAbilities),
            new("Rook",   15, 1, 1, FacingDirection.PlusY, rookMovement,   noAbilities),
            new("Knight", 15, 1, 1, FacingDirection.PlusY, knightMovement, noAbilities),
            new("Bishop", 15, 1, 1, FacingDirection.PlusY, bishopMovement, noAbilities),
            new("Queen",  20, 1, 1, FacingDirection.PlusY, queenMovement,  noAbilities),
            new("King",   25, 1, 1, FacingDirection.PlusY, kingMovement,   noAbilities),
        };
    }
}