#nullable enable

using System;
using System.Collections.Generic;

public static class StandardPieceTemplates
{
    public static IReadOnlyList<PieceTemplate> CreateAll(
        DeckSequenceProvider deckSequences,
        IEventBus eventBus)
    {
        var noMelee = Array.Empty<IMeleeAttack>();
        var noRanged = Array.Empty<IRangedAttack>();
        var noAbilities = Array.Empty<IAbility>();
        var noMovements = Array.Empty<IMovementFactory>();

        // Король: 8 клеток вокруг в плоскости X-Y.
        var kingMovement = new IMovementFactory[]
        {
            new MovementFactory<RelativeArea<Square>, Square>(
                new RelativeArea<Square>(MultipleAxes.Two, isCircle: false))
        };

        return new List<PieceTemplate>
        {
            new("Pawn",   10, noMovements,    noMelee, noRanged, noAbilities),
            new("Rook",   15, noMovements,    noMelee, noRanged, noAbilities),
            new("Knight", 15, noMovements,    noMelee, noRanged, noAbilities),
            new("Bishop", 15, noMovements,    noMelee, noRanged, noAbilities),
            new("Queen",  20, noMovements,    noMelee, noRanged, noAbilities),
            new("King",   25, kingMovement,   noMelee, noRanged, noAbilities),
        };
    }
}