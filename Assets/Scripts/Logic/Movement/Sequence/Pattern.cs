#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public readonly struct Pattern<TSquare> : ISequence<TSquare, Pattern<TSquare>> where TSquare : struct, ISquare<TSquare>
{
    private static readonly int[] _fullRange = { -1, 0, 1 };
    private static readonly int[] _zeroRange = { 0 };
    private readonly int _cachedHash;
    private readonly ImmutableDictionary<TSquare, Stayable> _squares;

    public Pattern(TSquare square)
    {
        var builder = ImmutableDictionary.CreateBuilder<TSquare, Stayable>();
        builder.Add(new TSquare(), Stayable.NotStay);
        var center = new TSquare();
        if (square.IsAdjacent(center))
            builder.Add(square, Stayable.Stay);
        else
        {
            var fallback = center.WithOffset(1, 0, 0, 0);
            builder.Add(fallback, Stayable.Stay);
        }
        _squares = builder.ToImmutable();
        CountOfStayables = 1;
        GuaranteesAtLeastOneStayable = true;
        var hash = new HashCode();
        foreach (var kv in _squares)
        {
            hash.Add(kv.Key);
            hash.Add(kv.Value);
        }
        _cachedHash = HashCode.Combine(hash.ToHashCode(), GuaranteesAtLeastOneStayable);
    }
    public Pattern(ImmutableDictionary<TSquare, Stayable> squares, bool guarantee = false) : this(squares, guarantee, true) { }
    private Pattern(ImmutableDictionary<TSquare, Stayable> squares, bool guarantee, bool recalculate)
    {
        ImmutableDictionary<TSquare, Stayable> finalSquares;
        if (recalculate)
        {
            var connected = GetConnectedToOrigin(squares.Keys);
            var builder = ImmutableDictionary.CreateBuilder<TSquare, Stayable>();
            foreach (var s in connected)
            {
                var stayable = s.IsZero ? Stayable.NotStay : squares[s];
                builder.Add(s, stayable);
            }
            finalSquares = builder.ToImmutable();
        }
        else
        {
            finalSquares = squares;
        }

        _squares = finalSquares;
        CountOfStayables = finalSquares.Values.Count(v => v == Stayable.Stay);
        GuaranteesAtLeastOneStayable = guarantee;
        var hash = new HashCode();
        foreach (var kv in _squares)
        {
            hash.Add(kv.Key);
            hash.Add(kv.Value);
        }
        _cachedHash = HashCode.Combine(hash.ToHashCode(), GuaranteesAtLeastOneStayable);
        if (GuaranteesAtLeastOneStayable && (Count == 1 || CountOfStayables < 1))
        {
            var firstCell = new TSquare().WithOffset(1, 0, 0, 0);
            _squares = _squares.SetItem(firstCell, Stayable.Stay);
            CountOfStayables = _squares.Values.Count(v => v == Stayable.Stay);
            hash = new HashCode();
            foreach (var kv in _squares)
            {
                hash.Add(kv.Key);
                hash.Add(kv.Value);
            }
            _cachedHash = HashCode.Combine(hash.ToHashCode(), GuaranteesAtLeastOneStayable);
        }
    }

    public bool GuaranteesAtLeastOneStayable { get; }
    public int Count => _squares.Count;
    public int CountOfStayables { get; }
    public ImmutableDictionary<TSquare, Stayable> Values => _squares;
    
    private ImmutableArray<KeyValuePair<TSquare, Stayable>>? ComputeAdded(IEnumerable<TSquare> newSquares, Stayable stayable)
    {
        var squares = _squares;
        var builder = ImmutableArray.CreateBuilder<KeyValuePair<TSquare, Stayable>>();
        var seen = new HashSet<TSquare>();

        foreach (var s in newSquares)
        {
            if (s.IsZero) continue;
            if (squares.ContainsKey(s)) continue;
            if (!seen.Add(s)) continue;

            bool touchesPattern = false;
            foreach (var neighbor in GetNeighbors(s))
            {
                if (squares.ContainsKey(neighbor))
                {
                    touchesPattern = true;
                    break;
                }
            }

            if (touchesPattern)
                builder.Add(new KeyValuePair<TSquare, Stayable>(s, stayable));
        }

        return builder.Count == 0 ? null : builder.ToImmutable();
    }
    private (HashSet<TSquare>? forRemove, ImmutableDictionary<TSquare, Stayable>? newSquares) ComputeRemoval(IEnumerable<TSquare> ienum)
    {
        if (_squares.Count == 1) return (null, null);

        var squares = _squares;

        var forRemove = ienum.Where(s => !s.IsZero && squares.ContainsKey(s)).ToHashSet();
        if (forRemove.Count == 0) return (null, null);

        return (forRemove, squares.RemoveRange(forRemove));
    }
    private HashSet<TSquare>? ComputeForChange(IEnumerable<TSquare> ienum)
    {
        var squares = _squares;
        var forChange = ienum.Where(s => !s.IsZero && squares.ContainsKey(s)).ToHashSet();
        return forChange.Count == 0 ? null : forChange;
    }

    private static (int[] dy, int[] dz, int[] dw) GetRanges(MultipleAxes axes) => axes switch
    {
        MultipleAxes.None or MultipleAxes.One => (_zeroRange, _zeroRange, _zeroRange),
        MultipleAxes.Two => (_fullRange, _zeroRange, _zeroRange),
        MultipleAxes.Three => (_fullRange, _fullRange, _zeroRange),
        MultipleAxes.Four => (_fullRange, _fullRange, _fullRange),
        _ => throw new ArgumentOutOfRangeException(nameof(axes), axes, "Найдены оси MultipleAxes, которые не поддерживаются")
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
        queue.Enqueue(zero);
        visited.Add(zero);

        var directions = GetNeighbors(zero).ToArray();

        while (queue.Count > 0)
        {
            TSquare current = queue.Dequeue();
            result.Add(current);

            foreach (var dir in directions)
            {
                TSquare neighbor = current.Add(dir);

                if (cellSet.Contains(neighbor) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return result;
    }

    public Pattern<TSquare> WithAdded(TSquare square, Stayable stayable) => WithAdded(new TSquare[] { square }, stayable);
    public Pattern<TSquare> WithRemoved(TSquare square) => WithRemoved(new TSquare[] { square });
    public Pattern<TSquare> WithSetStayable(TSquare square, Stayable stayable) => WithSetStayable(new TSquare[] { square }, stayable);
    public (ImmutableDictionary<TSquare, Stayable> squares, bool guarantee) DataWithAdded(TSquare square, Stayable stayable) => DataWithAdded(new TSquare[] { square }, stayable);
    public (ImmutableDictionary<TSquare, Stayable> squares, bool guarantee) DataWithRemoved(TSquare square) => DataWithRemoved(new TSquare[] { square });
    public (ImmutableDictionary<TSquare, Stayable> squares, bool guarantee) DataWithSetStayable(TSquare square, Stayable stayable) => DataWithSetStayable(new TSquare[] { square }, stayable);

    public Pattern<TSquare> WithAdded(IEnumerable<TSquare> newSquares, Stayable stayable)
    {
        var added = ComputeAdded(newSquares, stayable);
        if (added == null) return this;
        return new Pattern<TSquare>(_squares.SetItems(added.Value), GuaranteesAtLeastOneStayable, recalculate: false);
    }
    public (ImmutableDictionary<TSquare, Stayable> squares, bool guarantee) DataWithAdded(IEnumerable<TSquare> newSquares, Stayable stayable)
    {
        var added = ComputeAdded(newSquares, stayable);
        if (added == null) return (_squares, GuaranteesAtLeastOneStayable);
        return (_squares.SetItems(added.Value), false);
    }
    public Pattern<TSquare> WithRemoved(IEnumerable<TSquare> ienum)
    {
        var (forRemove, newSquares) = ComputeRemoval(ienum);
        if (forRemove == null) return this;

        var squares = _squares;

        if (CountOfStayables == 1 && forRemove.Any(s => squares[s] == Stayable.Stay)) return this;
        return new Pattern<TSquare>(newSquares!, GuaranteesAtLeastOneStayable, recalculate: true);
    }
    public (ImmutableDictionary<TSquare, Stayable> squares, bool guarantee) DataWithRemoved(IEnumerable<TSquare> ienum)
    {
        var (forRemove, newSquares) = ComputeRemoval(ienum);
        if (forRemove == null) return (_squares, GuaranteesAtLeastOneStayable);

        return (newSquares!, false);
    }
    public Pattern<TSquare> WithSetStayable(IEnumerable<TSquare> ienum, Stayable stayable)
    {
        var squares = _squares;
        var forChange = ComputeForChange(ienum);
        if (forChange == null) return this;

        if (stayable == Stayable.NotStay && CountOfStayables == 1 && forChange.Any(s => squares[s] == Stayable.Stay))
            return this;

        var newDict = _squares.SetItems(forChange.Select(s => new KeyValuePair<TSquare, Stayable>(s, stayable)));
        return new Pattern<TSquare>(newDict, GuaranteesAtLeastOneStayable, recalculate: false);
    }
    public (ImmutableDictionary<TSquare, Stayable> squares, bool guarantee) DataWithSetStayable(IEnumerable<TSquare> ienum, Stayable stayable)
    {
        var forChange = ComputeForChange(ienum);
        if (forChange == null) return (_squares, GuaranteesAtLeastOneStayable);

        var newDict = _squares.SetItems(forChange.Select(s => new KeyValuePair<TSquare, Stayable>(s, stayable)));
        return (newDict, false);
    }

    public override string ToString() => $"Паттерн с {Count} клетками, из них {CountOfStayables} стоячих";
    public Pattern<TSquare> Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;
    public ISequenceEnumerator<TSquare> GetEnumerator(TSquare start) => new Enumerable(_squares, start);

    public bool Equals(Pattern<TSquare> other)
    {
        if (ReferenceEquals(_squares, other._squares)) return true;
        if (_squares.Count != other._squares.Count) return false;

        foreach (var kv in _squares)
        {
            if (!other._squares.TryGetValue(kv.Key, out var otherValue))
                return false;
            if (!EqualityComparer<Stayable>.Default.Equals(kv.Value, otherValue))
                return false;
        }
        return true;
    }
    public override bool Equals(object? obj) => obj is Pattern<TSquare> other && Equals(other);
    public override int GetHashCode() => _cachedHash;

    public static bool operator ==(Pattern<TSquare> left, Pattern<TSquare> right) => left.Equals(right);
    public static bool operator !=(Pattern<TSquare> left, Pattern<TSquare> right) => !(left == right);

    private class Enumerable : SequenceEnumerable<TSquare>
    {
        private readonly ImmutableDictionary<TSquare, Stayable> _baseSquares;
        private Dictionary<TSquare, Stayable>? _allPositionsCache;
        private List<TSquare>? _onlyStayablesCache;

        public Enumerable(ImmutableDictionary<TSquare, Stayable> baseSquares, TSquare startSquare) : base(startSquare) => _baseSquares = baseSquares;

        protected override Dictionary<TSquare, Stayable> CreateAllPossiblePositions
        {
            get
            {
                if (_allPositionsCache != null) return _allPositionsCache;
                var dict = new Dictionary<TSquare, Stayable> { [StartPosition] = Stayable.NotStay };
                foreach (var (relPos, stayable) in _baseSquares)
                {
                    var absPos = StartPosition.Add(relPos);
                    if (absPos.Equals(StartPosition))
                        dict[absPos] = Stayable.NotStay;
                    else
                        dict[absPos] = stayable;
                }
                _allPositionsCache = dict;
                return dict;
            }
        }
        protected override List<TSquare> CreateOnlyStayablesPositions
        {
            get
            {
                if (_onlyStayablesCache != null) return _onlyStayablesCache;
                _onlyStayablesCache = CreateAllPossiblePositions
                    .Where(kvp => kvp.Value == Stayable.Stay)
                    .Select(kvp => kvp.Key)
                    .ToList();
                return _onlyStayablesCache;
            }
        }

        protected override bool ProtectedCanMoveTo(IPath<TSquare> path)
        {
            var positions = path.Positions;
            if (positions.Count < 2) return false;
            var all = CreateAllPossiblePositions;
            foreach (var pos in positions)
                if (!all.ContainsKey(pos))
                    return false;
            return all.TryGetValue(positions[^1], out var stayable) && stayable == Stayable.Stay;
        }
        public override bool CanMoveTo(TSquare target)
        {
            return CreateAllPossiblePositions.TryGetValue(target, out var stayable) && stayable == Stayable.Stay;
        }
    }
}