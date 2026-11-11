#nullable enable

using System.Collections.Generic;
using System.Linq;

public sealed class RemoverAttackableOfPathSet : ModificatorOfPathSet
{
    public override ShapeTarget Target => ShapeTarget.Attack;
    public HashSet<Square> Squares { get; }

    public RemoverAttackableOfPathSet(IEnumerable<Square> squares)
    {
        Squares = squares.ToHashSet();
    }

    public override PathSetPattern Apply(PathSetPattern sequence)
        => sequence.WithRemovedAttackable(Squares);
}