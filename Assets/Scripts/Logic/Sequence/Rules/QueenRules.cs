#nullable enable

using System.Collections.Generic;

public static class QueenRules
{
    public static IReadOnlyList<MoveRule> Create()
    {
        var rook = RookRules.Create();
        var bishop = BishopRules.Create();
        var result = new List<MoveRule>(rook.Count + bishop.Count);
        result.AddRange(rook);
        result.AddRange(bishop);
        return result;
    }
}