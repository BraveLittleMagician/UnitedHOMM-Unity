#nullable enable

using System.Collections.Generic;

public static class StandardPieceTemplates
{
    public static IReadOnlyList<PieceTemplate> CreateAll(DeckSequenceProvider deckSequences, IEventBus eventBus)
    {
        var noMovements = System.Array.Empty<IMovementFactory>();
        var noMelee = System.Array.Empty<IMeleeAttack>();
        var noRanged = System.Array.Empty<IRangedAttack>();
        var noAbilities = System.Array.Empty<IAbility>();

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
                movementFactories: noMovements,
                meleeAttacks: noMelee,
                rangedAttacks: noRanged,
                abilities: noAbilities),
        };
    }
}