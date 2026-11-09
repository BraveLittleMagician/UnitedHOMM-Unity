#nullable enable

using System.Collections.Generic;
using System.Linq;

public class IncreaserNotStayableOfPattern<TSquare> : ModificatorOfPattern<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public HashSet<TSquare> Squares { get; }
    public IncreaserNotStayableOfPattern(IEnumerable<TSquare> squares) => Squares = squares.ToHashSet();
    public override Pattern<TSquare> Apply(Pattern<TSquare> sequence) => sequence.WithAdded(Squares, Stayable.NotStay);
}