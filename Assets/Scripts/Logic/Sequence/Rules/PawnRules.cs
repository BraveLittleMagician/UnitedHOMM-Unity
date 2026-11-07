#nullable enable
using System.Collections.Immutable;

public static class PawnRules
{
    /// <summary>
    /// Composite move-shape: только forward Stay. Атака приходит из отдельной
    /// attack-shape (<see cref="CreateAttackShape"/> или <c>KingCellSet.Create()</c>).
    /// </summary>
    /// <remarks>
    /// Используй с
    /// <c>SequenceMovementResolver.ForCombined(registry, mods, CreateMoveShape(), CreateAttackShape())</c>.
    /// </remarks>
    public static Pattern<Square> CreateMoveShape()
    {
        var stayables = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        stayables[Square.Zero] = Stayable.NotStay;
        stayables[new Square { X = 0, Y = 1, Z = 0 }] = Stayable.Stay;
        return new Pattern<Square>(stayables.ToImmutable(), guarantee: true);
    }

    /// <summary>
    /// Composite attack-shape: диагонали как Attack (классический Pawn),
    /// forward как NotStay (waypoint для <c>AdderAttackableOfPattern((0,1,0))</c>,
    /// который может добавить forward в attack через свой <c>Target = Attack</c>).
    /// </summary>
    /// <remarks>
    /// Если нужно, чтобы Pawn атаковал только диагонали (классические шахматы),
    /// не навешивай <c>AdderAttackableOfPattern((0,1,0))</c> в JSON.
    /// Если навешан — Pawn атакует forward + 2 диагонали (3 клетки).
    /// </remarks>
    public static Pattern<Square> CreateAttackShape()
    {
        var stayables = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        var attackables = ImmutableDictionary.CreateBuilder<Square, Attackable>();

        stayables[Square.Zero] = Stayable.NotStay;

        var forward = new Square { X = 0, Y = 1, Z = 0 };
        stayables[forward] = Stayable.NotStay;

        var diagLeft = new Square { X = -1, Y = 1, Z = 0 };
        var diagRight = new Square { X = 1, Y = 1, Z = 0 };
        stayables[diagLeft] = Stayable.NotStay;
        stayables[diagRight] = Stayable.NotStay;
        attackables[diagLeft] = Attackable.Attack;
        attackables[diagRight] = Attackable.Attack;

        return new Pattern<Square>(stayables.ToImmutable(), attackables.ToImmutable(), guarantee: true);
    }
}