#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public readonly struct Pattern<TSquare> : ISequence<Square, Pattern<TSquare>>
    where TSquare : struct, ISquare<TSquare>
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

    private static readonly Dictionary<MultipleAxes, (int[] dy, int[] dz, int[] dw)> _rangeCache = new()
    {
        [MultipleAxes.None] = (_zeroRange, _zeroRange, _zeroRange),
        [MultipleAxes.One] = (_zeroRange, _zeroRange, _zeroRange),
        [MultipleAxes.Two] = (_fullRange, _zeroRange, _zeroRange),
        [MultipleAxes.Three] = (_fullRange, _fullRange, _zeroRange),
        [MultipleAxes.Four] = (_fullRange, _fullRange, _fullRange),
    };

    private static IEnumerable<TSquare> GetNeighbors(TSquare pos)
    {
        var (dyRange, dzRange, dwRange) = _rangeCache[pos.ActiveAxes];

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

        var directions = GetNeighbors(zero);

        while (queue.Count > 0)
        {
            TSquare current = queue.Dequeue();
            result.Add(current);

            foreach (var dir in directions)
            {
                TSquare neighbor = current.WithOffset(dir);

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
        var squares = _squares;
        var newAllowed = newSquares.Where(s => !s.IsZero && !squares.ContainsKey(s) && squares.Any(p => p.Key.IsAdjacent(s))).Select(s => new KeyValuePair<TSquare, Stayable>(s, stayable)).ToImmutableArray();
        if (newAllowed.Length == 0) return this;
        return new Pattern<TSquare>(squares.SetItems(newAllowed), GuaranteesAtLeastOneStayable, recalculate: false);
    }
    public (ImmutableDictionary<TSquare, Stayable> squares, bool guarantee) DataWithAdded(IEnumerable<TSquare> newSquares, Stayable stayable)
    {
        var squares = _squares;
        var newAllowed = newSquares.Where(s => !s.IsZero && !squares.ContainsKey(s) && squares.Any(p => p.Key.IsAdjacent(s))).Select(s => new KeyValuePair<TSquare, Stayable>(s, stayable)).ToImmutableArray();
        if (newAllowed.Length == 0) return (squares, GuaranteesAtLeastOneStayable);
        return (squares.SetItems(newAllowed), false);
    }
    public Pattern<TSquare> WithRemoved(IEnumerable<TSquare> ienum)
    {
        if (_squares.Count == 1) return this;
        ImmutableDictionary<TSquare, Stayable> squares = _squares;
        var forRemove = ienum.Where(s => !s.IsZero && squares.ContainsKey(s)).ToHashSet();
        if (forRemove.Count == 0 || (CountOfStayables == 1 && forRemove.Any(s => squares[s] == Stayable.Stay))) return this;
        return new Pattern<TSquare>(squares.RemoveRange(forRemove), GuaranteesAtLeastOneStayable, recalculate: true);
    }
    public (ImmutableDictionary<TSquare, Stayable> squares, bool guarantee) DataWithRemoved(IEnumerable<TSquare> ienum)
    {
        (ImmutableDictionary<TSquare, Stayable> squares, bool guaranteesAtLeastOneStayable) data = (_squares, GuaranteesAtLeastOneStayable);
        if (_squares.Count == 1) return data;
        var forRemove = ienum.Where(s => !s.IsZero && data.squares.ContainsKey(s)).ToHashSet();
        if (forRemove.Count == 0) return data;
        return (data.squares.RemoveRange(forRemove), false);
    }
    public Pattern<TSquare> WithSetStayable(IEnumerable<TSquare> ienum, Stayable stayable)
    {
        var squares = _squares;
        var forChange = ienum.Where(s => !s.IsZero && squares.ContainsKey(s)).ToHashSet();
        if (forChange.Count == 0) return this;
        if (stayable == Stayable.NotStay && CountOfStayables == 1 && forChange.Any(s => squares[s] == Stayable.Stay)) return this;
        var items = forChange.Select(s => new KeyValuePair<TSquare, Stayable>(s, stayable));
        var newDict = squares.SetItems(items);
        return new Pattern<TSquare>(newDict, GuaranteesAtLeastOneStayable, recalculate: false);
    }
    public (ImmutableDictionary<TSquare, Stayable> squares, bool guarantee) DataWithSetStayable(IEnumerable<TSquare> ienum, Stayable stayable)
    {
        (ImmutableDictionary<TSquare, Stayable> squares, bool guaranteesAtLeastOneStayable) data = (_squares, GuaranteesAtLeastOneStayable);
        var forChange = ienum.Where(s => !s.IsZero && data.squares.ContainsKey(s)).ToHashSet();
        if (forChange.Count == 0) return data;
        var newDict = data.squares.SetItems(forChange.Select(s => new KeyValuePair<TSquare, Stayable>(s, stayable)));
        return (newDict, false);
    }

    public override string ToString() => $"Паттерн с {Count} клетками, из них {CountOfStayables} стоячих";
    public Pattern<TSquare> Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;
    public ISequenceEnumerator<Square> GetEnumerator(Square start) => new Enumerable(_squares, start);

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

    private class Enumerable : SequenceEnumerable<Square>
    {
        private readonly ImmutableDictionary<TSquare, Stayable> _baseSquares;
        private readonly Square _start;
        private Dictionary<Square, Stayable>? _allPositionsCache;
        private List<Square>? _onlyStayablesCache;

        public Enumerable(ImmutableDictionary<TSquare, Stayable> baseSquares, Square startSquare)
            : base(startSquare)
        {
            _baseSquares = baseSquares;
            _start = startSquare;
        }

        protected override Dictionary<Square, Stayable> CreateAllPossiblePositions
        {
            get
            {
                if (_allPositionsCache != null) return _allPositionsCache;
                var dict = new Dictionary<Square, Stayable> { [_start] = Stayable.NotStay };
                foreach (var (relPos, stayable) in _baseSquares)
                {
                    var absPos = Translate(relPos);
                    if (absPos.Equals(_start))
                        dict[absPos] = Stayable.NotStay;
                    else
                        dict[absPos] = stayable;
                }
                _allPositionsCache = dict;
                return dict;
            }
        }
        protected override List<Square> CreateOnlyStayablesPositions
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

        private Square Translate(TSquare relative)
        {
            int x = _start.X;
            int y = _start.Y;
            int z = _start.Z;
            int w = _start.W;

            foreach (var pair in relative.Coordinates())
            {
                switch (pair.Key)
                {
                    case Axis.X: x += pair.Value; break;
                    case Axis.Y: y += pair.Value; break;
                    case Axis.Z: z += pair.Value; break;
                    case Axis.W: w += pair.Value; break;
                }
            }

            return new Square { X = x, Y = y, Z = z, W = w };
        }

        protected override bool ProtectedCanMoveTo(IPath<Square> path)
        {
            var positions = path.Positions;
            if (positions.Count < 2) return false;
            var all = CreateAllPossiblePositions;
            foreach (var pos in positions)
                if (!all.ContainsKey(pos))
                    return false;
            return all.TryGetValue(positions[^1], out var stayable) && stayable == Stayable.Stay;
        }
        public override bool CanMoveTo(Square target)
        {
            return CreateAllPossiblePositions.TryGetValue(target, out var stayable) && stayable == Stayable.Stay;
        }
    }
}