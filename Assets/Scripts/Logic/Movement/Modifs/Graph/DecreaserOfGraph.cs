#nullable enable

using System.Collections.Generic;

public class DecreaserOfGraph<TSquare> : ModificatorOfGraph<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public IEnumerable<TSquare> Positions { get; }
    public DecreaserOfGraph(IEnumerable<TSquare> positions) => Positions = positions;
    public override RelativeGraph<TSquare> Apply(RelativeGraph<TSquare> sequence)
    {
        foreach (var square in Positions)
            sequence.Remove(square);
        return sequence;
    }
}