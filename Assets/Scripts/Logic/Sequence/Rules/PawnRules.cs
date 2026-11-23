#nullable enable
using System.Collections.Immutable;

public static class PawnRules
{
    public static Pattern<Square> CreateMoveShape()
    {
        var stayables = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        stayables[Square.Zero] = Stayable.NotStay;
        stayables[new Square { X = 0, Y = 1, Z = 0 }] = Stayable.Stay;
        return new Pattern<Square>(stayables.ToImmutable());
    }

    public static Pattern<Square> CreateAttackShape()
    {
        var stayables = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        stayables[Square.Zero] = Stayable.NotStay;
        stayables[new Square { X = 0, Y = 1, Z = 0 }] = Stayable.NotStay;
        stayables[new Square { X = -1, Y = 1, Z = 0 }] = Stayable.Stay;
        stayables[new Square { X = 1, Y = 1, Z = 0 }] = Stayable.Stay;
        return new Pattern<Square>(stayables.ToImmutable());
    }
}