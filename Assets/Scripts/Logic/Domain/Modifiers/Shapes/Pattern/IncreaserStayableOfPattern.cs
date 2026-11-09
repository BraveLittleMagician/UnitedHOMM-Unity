#nullable enable

using System.Collections.Generic;
using System.Linq;

public class IncreaserStayableOfPattern<TSquare> : ModificatorOfPattern<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public HashSet<TSquare> Squares { get; }
    public IncreaserStayableOfPattern(IEnumerable<TSquare> squares) => Squares = squares.ToHashSet();
    public override Pattern<TSquare> Apply(Pattern<TSquare> sequence) => sequence.WithAdded(Squares, Stayable.Stay);
}