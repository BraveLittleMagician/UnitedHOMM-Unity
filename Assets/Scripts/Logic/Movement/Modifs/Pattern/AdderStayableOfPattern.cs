#nullable enable

using System.Collections.Generic;
using System.Linq;

public class AdderStayableOfPattern<TSquare> : ModificatorOfPattern<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public HashSet<TSquare> Squares { get; }
    public AdderStayableOfPattern(IEnumerable<TSquare> squares) => Squares = squares.ToHashSet();
    public override Pattern<TSquare> Apply(Pattern<TSquare> sequence) => sequence.WithSetStayable(Squares, Stayable.Stay);
}