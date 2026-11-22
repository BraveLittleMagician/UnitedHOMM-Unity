#nullable enable

using System.Collections.Generic;

public interface ISequenceEnumerator<TPosition> where TPosition : struct
{
    bool SupportsExplicitAttackable { get; }
    int MaxDepth { get; }
    TPosition StartPosition { get; }
    Dictionary<TPosition, Stayable> PositionsAll { get; }
    List<TPosition> PositionsStayables { get; }
    IReadOnlyDictionary<TPosition, Attackable> PositionsAttackable { get; }

    bool CanMoveTo(TPosition target);
    bool CanMoveTo(IPath<TPosition> path);
    IEnumerable<TPosition> GetNeighbors(TPosition cell);
    IPath<TPosition> CombinePositionsIntoPath(TPosition[] positions);


    IReadOnlyDictionary<TPosition, SquareInfo> Squares
    {
        get
        {
            var all = PositionsAll;
            var attack = PositionsAttackable;
            var result = new Dictionary<TPosition, SquareInfo>(all.Count);
            foreach (var kv in all)
            {
                var a = attack.TryGetValue(kv.Key, out var av) ? av : Attackable.NotAttack;
                result[kv.Key] = new SquareInfo(kv.Value, a);
            }
            return result;
        }
    }

    IReadOnlyCollection<TPosition> StaySquares
    {
        get
        {
            var list = new List<TPosition>();
            foreach (var kv in Squares) if (kv.Value.CanStopAt) list.Add(kv.Key);
            return list;
        }
    }

    IReadOnlyCollection<TPosition> AttackSquares
    {
        get
        {
            var list = new List<TPosition>();
            foreach (var kv in Squares) if (kv.Value.IsAttackable) list.Add(kv.Key);
            return list;
        }
    }

    bool CanAttack(TPosition target) => PositionsAttackable.ContainsKey(target);

    bool CanAttack(IPath<TPosition> path)
    {
        var positions = path.Positions;
        if (positions.Count < 2) return false;
        if (!positions[0].Equals(StartPosition)) return false;

        var attackable = PositionsAttackable;
        for (int i = 1; i < positions.Count; i++)
            if (!attackable.ContainsKey(positions[i])) return false;

        for (int i = 0; i < positions.Count - 1; i++)
        {
            bool connected = false;
            foreach (var n in GetNeighbors(positions[i]))
                if (n.Equals(positions[i + 1])) { connected = true; break; }
            if (!connected) return false;
        }
        return true;
    }
}