#nullable enable

using System.Collections.Generic;

public class SetStayableOfGraph<TSquare> : ModificatorOfGraph<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public IEnumerable<(TSquare position, Stayable stayable)> Cells { get; }
    public SetStayableOfGraph(IEnumerable<(TSquare, Stayable)> cells) => Cells = cells;
    public override RelativeGraph<TSquare> Apply(RelativeGraph<TSquare> sequence)
    {
        var copy = sequence.Copy();
        foreach (var (position, stayable) in Cells)
            copy.SetStayable(position, stayable);
        return copy;
    }
}