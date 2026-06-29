#nullable enable

using System.Collections.Generic;
using System.Linq;

public class DecreaserOfPattern<TSquare> : ModificatorOfPattern<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public HashSet<TSquare> Squares { get; }
    public DecreaserOfPattern(IEnumerable<TSquare> squares) => Squares = squares.ToHashSet();
    public override Pattern<TSquare> Apply(Pattern<TSquare> sequence)
    {
        var (squares, guarantee) = sequence.DataWithRemoved(Squares);
        return new Pattern<TSquare>(squares, guarantee);
    }
}