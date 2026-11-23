#nullable enable

using System.Collections.Generic;
using System.Linq;

public class RemoverStayableOfPattern<TSquare> : ModificatorOfPattern<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public HashSet<TSquare> Squares { get; }
    public RemoverStayableOfPattern(IEnumerable<TSquare> squares) => Squares = squares.ToHashSet();
    public override Pattern<TSquare> Apply(Pattern<TSquare> sequence)
    {
        var (squares, guarantee) = sequence.DataWithSetStayable(Squares, Stayable.NotStay);
        return new Pattern<TSquare>(squares, guarantee);
    }
}