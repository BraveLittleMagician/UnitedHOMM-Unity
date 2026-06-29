#nullable enable

using System.Collections.Generic;

public class IncreaserOfGraph<TSquare> : ModificatorOfGraph<TSquare> where TSquare : struct, ISquare<TSquare>
{
    public IEnumerable<(TSquare position, int length, IEnumerable<int> stayableIndices)> Nodes { get; }
    public IncreaserOfGraph(IEnumerable<(TSquare, int, IEnumerable<int>)> nodes) => Nodes = nodes;
    public override RelativeGraph<TSquare> Apply(RelativeGraph<TSquare> sequence)
    {
        foreach (var (pos, length, indices) in Nodes)
            sequence.Add(pos, length, indices);
        return sequence;
    }
}