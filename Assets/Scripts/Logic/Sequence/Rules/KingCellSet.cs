#nullable enable

using System.Collections.Immutable;

public static class KingCellSet
{
    public static Pattern<Square> Create()
    {
        var stayables = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        var attackables = ImmutableDictionary.CreateBuilder<Square, Attackable>();
        stayables[Square.Zero] = Stayable.NotStay;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;
                    var cell = new Square { X = dx, Y = dy, Z = dz };
                    stayables[cell] = Stayable.Stay;
                    attackables[cell] = Attackable.Attack;
                }
        return new Pattern<Square>(stayables.ToImmutable(), attackables.ToImmutable(), guarantee: true);
    }
}