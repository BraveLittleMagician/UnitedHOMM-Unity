#nullable enable

using System;
using System.Collections.Generic;

public static class StandardPieceTemplates
{
    public static IReadOnlyList<PieceTemplate> CreateAll(DeckSequenceProvider deckSequences, IEventBus eventBus)
    {
        var noMelee = Array.Empty<IMeleeAttack>();
        var noRanged = Array.Empty<IRangedAttack>();
        var noAbilities = Array.Empty<IAbility>();
        var noMovements = Array.Empty<IMovementFactory>();

        var kingMovement = new IMovementFactory[]
        {
            new MovementCombinedFactory<RelativeArea<Square>, Board, Square>(
                new RelativeArea<Square>(activeAxes: MultipleAxes.Two, isCircle: false))
        };

        return new List<PieceTemplate>
        {
            new(
                name: "Pawn",
                health: 10,
                movementFactories: noMovements,
                meleeAttacks: noMelee,
                rangedAttacks: noRanged,
                abilities: noAbilities),

            new(
                name: "Rook",
                health: 15,
                movementFactories: noMovements,
                meleeAttacks: noMelee,
                rangedAttacks: noRanged,
                abilities: noAbilities),

            new(
                name: "Knight",
                health: 15,
                movementFactories: noMovements,
                meleeAttacks: noMelee,
                rangedAttacks: noRanged,
                abilities: noAbilities),

            new(
                name: "Bishop",
                health: 15,
                movementFactories: noMovements,
                meleeAttacks: noMelee,
                rangedAttacks: noRanged,
                abilities: noAbilities),

            new(
                name: "Queen",
                health: 20,
                movementFactories: noMovements,
                meleeAttacks: noMelee,
                rangedAttacks: noRanged,
                abilities: noAbilities),

            new(
                name: "King",
                health: 25,
                movementFactories: kingMovement,
                meleeAttacks: noMelee,
                rangedAttacks: noRanged,
                abilities: noAbilities),
        };
    }
}