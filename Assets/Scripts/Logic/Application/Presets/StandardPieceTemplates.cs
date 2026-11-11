#nullable enable

using System;
using System.Collections.Generic;

public static class StandardPieceTemplates
{
    public static IReadOnlyList<PieceTemplate> CreateAll(DeckSequenceProvider deckSequences, IEventBus eventBus)
    {
        var noAbilities = Array.Empty<IAbility>();
        var noMovements = Array.Empty<IMovementFactory>();

        var kingMovement = new IMovementFactory[]
        {
            new MovementFactory<RelativeArea<Square>>(
                new RelativeArea<Square>(MultipleAxes.Two, isCircle: false))
        };

        return new List<PieceTemplate>
        {
            new("Pawn",   10, 1, 1, FacingDirection.PlusY, noMovements,   noAbilities),
            new("Rook",   15, 1, 1, FacingDirection.PlusY, noMovements,   noAbilities),
            new("Knight", 15, 1, 1, FacingDirection.PlusY, noMovements,   noAbilities),
            new("Bishop", 15, 1, 1, FacingDirection.PlusY, noMovements,   noAbilities),
            new("Queen",  20, 1, 1, FacingDirection.PlusY, noMovements,   noAbilities),
            new("King",   25, 1, 1, FacingDirection.PlusY, kingMovement,  noAbilities),
        };
    }
}