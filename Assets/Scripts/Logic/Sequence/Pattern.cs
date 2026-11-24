#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public readonly struct Pattern<TSquare, TPurpose> : ISequence<TSquare, TPurpose, Pattern<TSquare, TPurpose>> where TSquare : struct, ISquare<TSquare> where TPurpose : struct, Enum
{
    private static readonly TPurpose _activePurpose = (TPurpose)Enum.ToObject(typeof(TPurpose), Convert.ChangeType(1, Enum.GetUnderlyingType(typeof(TPurpose))));
    private static readonly int[] _fullRange = { -1, 0, 1 };
    private static readonly int[] _zeroRange = { 0 };
    private readonly int _cachedHash;
    private readonly ImmutableDictionary<TSquare, TPurpose> _squares;

    private Pattern(ImmutableDictionary<TSquare, TPurpose> squares, bool recalculate)
    {
        ImmutableDictionary<TSquare, TPurpose> finalSquares;
        if (recalculate)
        {
            var connected = GetConnectedToOrigin(squares.Keys);
            var sb = ImmutableDictionary.CreateBuilder<TSquare, TPurpose>();
            foreach (var s in connected)
                sb.Add(s, s.IsZero ? default : squares[s]);
            finalSquares = sb.ToImmutable();
        }
        else
            finalSquares = squares;

        _squares = finalSquares;

        CountOfAbles = finalSquares.Values.Count(static v => EqualityComparer<TPurpose>.Default.Equals(v, _activePurpose));
        MaxDepth = ComputeMaxDepth(finalSquares);
        _cachedHash = ComputeHash(finalSquares);
    }
    public Pattern(TSquare square)
    {
        var builder = ImmutableDictionary.CreateBuilder<TSquare, TPurpose>();
        var center = new TSquare();

        builder.Add(center, default);

        TSquare cell = square.IsAdjacent(center) ? square : center.WithOffset(1, 0, 0, 0);

        builder.Add(cell, _activePurpose);

        _squares = builder.ToImmutable();
        CountOfAbles = 1;
        MaxDepth = ComputeMaxDepth(_squares);
        _cachedHash = ComputeHash(_squares);
    }
    public Pattern(ImmutableDictionary<TSquare, TPurpose> squares) : this(squares, recalculate: true) { }

    public int MaxDepth { get; }
    public int Count => _squares.Count;
    public int CountOfAbles { get; }
    public ImmutableDictionary<TSquare, TPurpose> Values => _squares;

    private ImmutableArray<KeyValuePair<TSquare, TPurpose>>? ComputeAdded(IEnumerable<TSquare> newSquares, TPurpose purpose)
    {
        var squares = _squares;
        var builder = ImmutableArray.CreateBuilder<KeyValuePair<TSquare, TPurpose>>();
        var seen = new HashSet<TSquare>();
        foreach (var s in newSquares)
        {
            if (s.IsZero || squares.ContainsKey(s) || !seen.Add(s)) continue;
            bool touches = false;
            foreach (var neighbor in GetNeighbors(s))
                if (squares.ContainsKey(neighbor)) { touches = true; break; }
            if (touches) builder.Add(new KeyValuePair<TSquare, TPurpose>(s, purpose));
        }
        return builder.Count == 0 ? null : builder.ToImmutable();
    }
    private HashSet<TSquare>? ComputeForChange(IEnumerable<TSquare> ienum)
    {
        var squares = _squares;
        var forChange = ienum.Where(s => !s.IsZero && squares.ContainsKey(s)).ToHashSet();
        return forChange.Count == 0 ? null : forChange;
    }
    private (HashSet<TSquare>? forRemove, ImmutableDictionary<TSquare, TPurpose>? newSquares) ComputeRemoval(IEnumerable<TSquare> ienum)
    {
        if (_squares.Count == 1) return (null, null);
        var squares = _squares;
        var forRemove = ienum.Where(s => !s.IsZero && squares.ContainsKey(s)).ToHashSet();
        if (forRemove.Count == 0) return (null, null);
        return (forRemove, squares.RemoveRange(forRemove));
    }
    private static int ComputeHash(ImmutableDictionary<TSquare, TPurpose> squares)
    {
        var hash = new HashCode();
        foreach (var kv in squares) { hash.Add(kv.Key); hash.Add(kv.Value); }
        return hash.ToHashCode();
    }

    private static (int[] dy, int[] dz, int[] dw) GetRanges(MultipleAxes axes) => axes switch
    {
        MultipleAxes.None or MultipleAxes.One => (_zeroRange, _zeroRange, _zeroRange),
        MultipleAxes.Two => (_fullRange, _zeroRange, _zeroRange),
        MultipleAxes.Three => (_fullRange, _fullRange, _zeroRange),
        MultipleAxes.Four => (_fullRange, _fullRange, _fullRange),
        _ => throw new ArgumentOutOfRangeException(nameof(axes)),
    };
    private static IEnumerable<TSquare> GetNeighbors(TSquare pos)
    {
        var (dyRange, dzRange, dwRange) = GetRanges(pos.ActiveAxes);
        foreach (int dx in _fullRange)
            foreach (int dy in dyRange)
                foreach (int dz in dzRange)
                    foreach (int dw in dwRange)
                    {
                        if (dx == 0 && dy == 0 && dz == 0 && dw == 0) continue;
                        yield return pos.WithOffset(dx, dy, dz, dw);
                    }
    }
    private static HashSet<TSquare> GetConnectedToOrigin(IEnumerable<TSquare> cells)
    {
        var cellSet = new HashSet<TSquare>(cells);
        var result = new HashSet<TSquare>();
        var visited = new HashSet<TSquare>();
        var queue = new Queue<TSquare>();
        TSquare zero = new();
        queue.Enqueue(zero); visited.Add(zero);
        var dirs = GetNeighbors(zero).ToArray();
        while (queue.Count > 0)
        {
            TSquare cur = queue.Dequeue();
            result.Add(cur);
            foreach (var dir in dirs)
            {
                TSquare nb = cur.Add(dir);
                if (cellSet.Contains(nb) && !visited.Contains(nb)) { visited.Add(nb); queue.Enqueue(nb); }
            }
        }
        return result;
    }
    private static int ComputeMaxDepth(ImmutableDictionary<TSquare, TPurpose> squares)
    {
        var origin = new TSquare();
        if (!squares.ContainsKey(origin)) return 0;
        var visited = new HashSet<TSquare> { origin };
        var queue = new Queue<(TSquare square, int depth)>();
        queue.Enqueue((origin, 0));
        int maxDepth = 0;
        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (depth > maxDepth) maxDepth = depth;
            foreach (var neighbor in GetNeighbors(current))
            {
                if (!squares.ContainsKey(neighbor)) continue;
                if (!visited.Add(neighbor)) continue;
                queue.Enqueue((neighbor, depth + 1));
            }
        }
        return maxDepth;
    }

    public Pattern<TSquare, TPurpose> WithRemoved(TSquare square) => WithRemoved(new[] { square });
    public Pattern<TSquare, TPurpose> WithAdded(TSquare square, TPurpose purpose) => WithAdded(new[] { square }, purpose);
    public Pattern<TSquare, TPurpose> WithSetStayable(TSquare square, TPurpose purpose) => WithSetStayable(new[] { square }, purpose);
    public Pattern<TSquare, TPurpose> WithRemoved(IEnumerable<TSquare> ienum)
    {
        var (forRemove, newSquares) = ComputeRemoval(ienum);
        if (forRemove == null) return this;
        return new Pattern<TSquare, TPurpose>(newSquares!, recalculate: true);
    }
    public Pattern<TSquare, TPurpose> WithAdded(IEnumerable<TSquare> ienum, TPurpose purpose)
    {
        var added = ComputeAdded(ienum, purpose);
        if (added == null) return this;
        return new Pattern<TSquare, TPurpose>(_squares.SetItems(added.Value), recalculate: false);
    }
    public Pattern<TSquare, TPurpose> WithSetStayable(IEnumerable<TSquare> ienum, TPurpose purpose)
    {
        var forChange = ComputeForChange(ienum);
        if (forChange == null) return this;
        var newDict = _squares.SetItems(forChange.Select(s => new KeyValuePair<TSquare, TPurpose>(s, purpose)));
        return new Pattern<TSquare, TPurpose>(newDict, recalculate: false);
    }

    public override string ToString() => $"Паттерн с {Count} клетками, из них {CountOfAbles} активных";
    public Pattern<TSquare, TPurpose> Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;
    public ISequenceEnumerator<TSquare, TPurpose> GetEnumerator(TSquare start) => new Enumerable(_squares, start, MaxDepth);

    public bool Equals(Pattern<TSquare, TPurpose> other)
    {
        if (_squares == null) return other._squares == null;
        if (other._squares == null) return false;
        if (ReferenceEquals(_squares, other._squares)) return true;
        if (_squares.Count != other._squares.Count) return false;
        foreach (var kv in _squares)
        {
            if (!other._squares.TryGetValue(kv.Key, out var v)) return false;
            if (!EqualityComparer<TPurpose>.Default.Equals(kv.Value, v)) return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => obj is Pattern<TSquare, TPurpose> other && Equals(other);
    public override int GetHashCode() => _cachedHash;
    public static bool operator ==(Pattern<TSquare, TPurpose> left, Pattern<TSquare, TPurpose> right) => left.Equals(right);
    public static bool operator !=(Pattern<TSquare, TPurpose> left, Pattern<TSquare, TPurpose> right) => !(left == right);

    private class Enumerable : SequenceEnumerable<TSquare, TPurpose>
    {
        private readonly ImmutableDictionary<TSquare, TPurpose> _baseSquares;

        private Dictionary<TSquare, TPurpose>? _allPositionsCache;
        private List<TSquare>? _onlyAblesCache;

        public Enumerable(ImmutableDictionary<TSquare, TPurpose> baseSquares, TSquare startSquare, int maxDepth) : base(startSquare, maxDepth)
        {
            _baseSquares = baseSquares;
        }

        protected override bool ProtectedCanMoveTo(IPath<TSquare> path)
        {
            var positions = path.Positions;
            if (positions.Count < 2) return false;
            if (positions.Count - 1 > MaxDepth) return false;
            var all = CreateAllPositions;
            foreach (var pos in positions)
                if (!all.ContainsKey(pos)) return false;
            return all.TryGetValue(positions[^1], out var purpose) && EqualityComparer<TPurpose>.Default.Equals(purpose, _activePurpose);
        }
        protected override Dictionary<TSquare, TPurpose> CreateAllPositions
        {
            get
            {
                if (_allPositionsCache != null) return _allPositionsCache;
                var dict = new Dictionary<TSquare, TPurpose> { [StartPosition] = new ()};
                foreach (var (relPos, purpose) in _baseSquares)
                {
                    var absPos = StartPosition.Add(relPos);
                    if (absPos.Equals(StartPosition)) 
                        dict[absPos] = new ();
                    else dict[absPos] = purpose;
                }
                _allPositionsCache = dict;
                return dict;
            }
        }
        protected override List<TSquare> CreateOnlyAblesPositions
        {
            get
            {
                if (_onlyAblesCache != null) return _onlyAblesCache;
                _onlyAblesCache = CreateAllPositions
                    .Where(static kv => EqualityComparer<TPurpose>.Default.Equals(kv.Value, _activePurpose))
                    .Select(static kv => kv.Key).ToList();
                return _onlyAblesCache;
            }
        }

        public override bool CanReach(TSquare target) => CreateAllPositions.TryGetValue(target, out var purpose) && EqualityComparer<TPurpose>.Default.Equals(purpose, _activePurpose);

        public override IEnumerable<TSquare> GetNeighbors(TSquare square) => square.UnitNeighbors();
        public override IPath<TSquare> CombinePositionsIntoPath(TSquare[] positions) => new PathSequence<TSquare>(positions);
    }
}