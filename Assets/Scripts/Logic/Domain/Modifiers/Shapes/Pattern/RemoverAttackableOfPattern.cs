#nullable enable

using System.Collections.Generic;
using System.Linq;

public class RemoverAttackableOfPattern<TSquare> : ModificatorOfPattern<TSquare>
    where TSquare : struct, ISquare<TSquare>
{
    public override ShapeTarget Target => ShapeTarget.Attack;
    public HashSet<TSquare> Squares { get; }
    public RemoverAttackableOfPattern(IEnumerable<TSquare> squares) => Squares = squares.ToHashSet();

    public override Pattern<TSquare> Apply(Pattern<TSquare> sequence)
    {
        var (squares, attackable, guarantee) = sequence.DataWithRemovedAttackable(Squares);
        return new Pattern<TSquare>(squares, attackable, guarantee);
    }
}