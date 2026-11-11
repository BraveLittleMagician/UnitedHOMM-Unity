#nullable enable

using System.Collections.Generic;
using System.Linq;

public sealed class AdderAttackableOfPathSet : ModificatorOfPathSet
{
    public override ShapeTarget Target => ShapeTarget.Attack;
    public HashSet<Square> Squares { get; }

    public AdderAttackableOfPathSet(IEnumerable<Square> squares)
    {
        Squares = squares.ToHashSet();
    }

    public override PathSetPattern Apply(PathSetPattern sequence) => sequence.WithAddedAttackable(Squares, Attackable.Attack);
}