#nullable enable

using System.Collections.Generic;
using System.Linq;

public sealed class RemoverAttackableOfPattern<TSquare> : ModificatorOfPattern<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public override ShapeTarget Target => ShapeTarget.Attack;
    public HashSet<TSquare> Squares { get; }

    public RemoverAttackableOfPattern(IEnumerable<TSquare> squares)
    {
        Squares = squares.ToHashSet();
    }

    public override Pattern<TSquare> Apply(Pattern<TSquare> sequence)
        => sequence.WithRemovedAttackable(Squares);
}