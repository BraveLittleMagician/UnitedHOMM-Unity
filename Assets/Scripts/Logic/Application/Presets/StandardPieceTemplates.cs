#nullable enable

using System;
using System.Collections.Generic;

public static class StandardPieceTemplates
{
    public static IReadOnlyList<PieceTemplate> CreateAll(IEventBus eventBus)
    {
        var noMelee = Array.Empty<IMeleeAttack>();
        var noRanged = Array.Empty<IRangedAttack>();
        var noAbilities = Array.Empty<IAbility>();
        var noModifiers = Array.Empty<ModifierDto>();
        return new List<PieceTemplate>
        {
            new("Pawn",   10, 1, 1, "PawnMove", "PawnAttack", noMelee, noRanged, noAbilities, noModifiers),
            new("Rook",   15, 2, 1, "Rook", null, noMelee, noRanged, noAbilities, noModifiers),
            new("Knight", 15, 2, 1, "Knight", null, noMelee, noRanged, noAbilities, noModifiers),
            new("Bishop", 15, 2, 1, "Bishop", null, noMelee, noRanged, noAbilities, noModifiers),
            new("Queen",  20, 3, 1, "Queen", null, noMelee, noRanged, noAbilities, noModifiers),
            new("King",   25, 1, 1, "KingMove", "KingAttack", noMelee, noRanged, noAbilities, noModifiers),
        };
    }
}