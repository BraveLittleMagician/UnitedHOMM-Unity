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
    private readonly ImmutableDictionary<TSquare, Attackable> _attackables;

    private Pattern(ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee, bool recalculate)
    {
        ImmutableDictionary<TSquare, Stayable> finalSquares;
        ImmutableDictionary<TSquare, Attackable> finalAttack;
        if (recalculate)
        {
            var connected = GetConnectedToOrigin(squares.Keys);
            var sb = ImmutableDictionary.CreateBuilder<TSquare, Stayable>();
            foreach (var s in connected)
                sb.Add(s, s.IsZero ? Stayable.NotStay : squares[s]);
            finalSquares = sb.ToImmutable();

            var ab = ImmutableDictionary.CreateBuilder<TSquare, Attackable>();
            foreach (var (k, v) in attackable)
                if (connected.Contains(k)) ab.Add(k, v);
            finalAttack = ab.ToImmutable();
        }
        else
        {
            finalSquares = squares;
            finalAttack = attackable;
        }

        int countOfStayables = finalSquares.Values.Count(v => v == Stayable.Stay);

        if (guarantee && (finalSquares.Count == 1 || countOfStayables < 1))
        {
            var firstCell = new TSquare().WithOffset(1, 0, 0, 0);
            finalSquares = finalSquares.SetItem(firstCell, Stayable.Stay);
            countOfStayables = finalSquares.Values.Count(v => v == Stayable.Stay);
        }

        _squares = finalSquares;
        _attackables = finalAttack;
        CountOfStayables = countOfStayables;
        GuaranteesAtLeastOneStayable = guarantee;
        MaxDepth = ComputeMaxDepth(finalSquares);
        _cachedHash = ComputeHash(finalSquares, finalAttack, guarantee);
    }
    public Pattern(TSquare square) : this(square, Attackable.NotAttack) { }
    public Pattern(TSquare square, Attackable attackable)
    {
        var builder = ImmutableDictionary.CreateBuilder<TSquare, Stayable>();
        var attackBuilder = ImmutableDictionary.CreateBuilder<TSquare, Attackable>();
        builder.Add(new TSquare(), Stayable.NotStay);
        var center = new TSquare();
        TSquare cell;
        if (square.IsAdjacent(center)) cell = square;
        else cell = center.WithOffset(1, 0, 0, 0);
        builder.Add(cell, Stayable.Stay);
        if (attackable == Attackable.Attack) attackBuilder.Add(cell, Attackable.Attack);

        _squares = builder.ToImmutable();
        _attackables = attackBuilder.ToImmutable();
        CountOfStayables = 1;
        GuaranteesAtLeastOneStayable = true;
        MaxDepth = ComputeMaxDepth(_squares);
        _cachedHash = ComputeHash(_squares, _attackables, GuaranteesAtLeastOneStayable);
    }
    public Pattern(ImmutableDictionary<TSquare, Stayable> squares, bool guarantee = false) : this(squares, ImmutableDictionary<TSquare, Attackable>.Empty, guarantee) { }
    public Pattern(ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee = false) : this(squares, attackable, guarantee, recalculate: true) { }

    public bool GuaranteesAtLeastOneStayable { get; }
    public int Count => _squares.Count;
    public int MaxDepth { get; }
    public int CountOfStayables { get; }
    public ImmutableDictionary<TSquare, Stayable> Values => _squares;
    public ImmutableDictionary<TSquare, Attackable> ValuesAttackable => _attackables;

    private static int ComputeHash(
        ImmutableDictionary<TSquare, Stayable> squares,
        ImmutableDictionary<TSquare, Attackable> attackable,
        bool guarantee)
    {
        var hash = new HashCode();
        hash.Add(guarantee);
        foreach (var kv in squares)
        {
            hash.Add(kv.Key);
            hash.Add(kv.Value);
        }
        foreach (var kv in attackable)
        {
            hash.Add(kv.Key);
            hash.Add(kv.Value);
        }
        return hash.ToHashCode();
    }

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
            bool touches = false;
            foreach (var neighbor in GetNeighbors(s))
                if (squares.ContainsKey(neighbor)) { touches = true; break; }
            if (touches) builder.Add(new KeyValuePair<TSquare, Stayable>(s, stayable));
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
    private static int ComputeMaxDepth(ImmutableDictionary<TSquare, Stayable> squares)
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

    public Pattern<TSquare> WithAdded(TSquare square, Stayable stayable) => WithAdded(new[] { square }, stayable);
    public Pattern<TSquare> WithRemoved(TSquare square) => WithRemoved(new[] { square });
    public Pattern<TSquare> WithSetStayable(TSquare square, Stayable stayable) => WithSetStayable(new[] { square }, stayable);

    public (ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee)
        DataWithAdded(TSquare square, Stayable stayable) => DataWithAdded(new[] { square }, stayable);
    public (ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee)
        DataWithRemoved(TSquare square) => DataWithRemoved(new[] { square });
    public (ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee)
        DataWithSetStayable(TSquare square, Stayable stayable) => DataWithSetStayable(new[] { square }, stayable);

    public Pattern<TSquare> WithAdded(IEnumerable<TSquare> newSquares, Stayable stayable)
    {
        var added = ComputeAdded(newSquares, stayable);
        if (added == null) return this;
        return new Pattern<TSquare>(_squares.SetItems(added.Value), _attackables, GuaranteesAtLeastOneStayable, recalculate: false);
    }
    public (ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee)
        DataWithAdded(IEnumerable<TSquare> newSquares, Stayable stayable)
    {
        var added = ComputeAdded(newSquares, stayable);
        if (added == null) return (_squares, _attackables, GuaranteesAtLeastOneStayable);
        return (_squares.SetItems(added.Value), _attackables, false);
    }
    public Pattern<TSquare> WithRemoved(IEnumerable<TSquare> ienum)
    {
        var (forRemove, newSquares) = ComputeRemoval(ienum);
        if (forRemove == null) return this;
        var squares = _squares;
        if (GuaranteesAtLeastOneStayable && CountOfStayables == 1 && forRemove.Any(s => squares[s] == Stayable.Stay)) return this;
        var newAttack = _attackables.RemoveRange(forRemove);
        return new Pattern<TSquare>(newSquares!, newAttack, GuaranteesAtLeastOneStayable, recalculate: true);
    }
    public (ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee)
        DataWithRemoved(IEnumerable<TSquare> ienum)
    {
        var (forRemove, newSquares) = ComputeRemoval(ienum);
        if (forRemove == null) return (_squares, _attackables, GuaranteesAtLeastOneStayable);
        return (newSquares!, _attackables.RemoveRange(forRemove), false);
    }
    public Pattern<TSquare> WithSetStayable(IEnumerable<TSquare> ienum, Stayable stayable)
    {
        var squares = _squares;
        var forChange = ComputeForChange(ienum);
        if (forChange == null) return this;
        if (stayable == Stayable.NotStay && GuaranteesAtLeastOneStayable && CountOfStayables == 1 && forChange.Any(s => squares[s] == Stayable.Stay)) return this;
        var newDict = _squares.SetItems(forChange.Select(s => new KeyValuePair<TSquare, Stayable>(s, stayable)));
        return new Pattern<TSquare>(newDict, _attackables, GuaranteesAtLeastOneStayable, recalculate: false);
    }
    public (ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee)
        DataWithSetStayable(IEnumerable<TSquare> ienum, Stayable stayable)
    {
        var forChange = ComputeForChange(ienum);
        if (forChange == null) return (_squares, _attackables, GuaranteesAtLeastOneStayable);
        var newDict = _squares.SetItems(forChange.Select(s => new KeyValuePair<TSquare, Stayable>(s, stayable)));
        return (newDict, _attackables, false);
    }
    public Pattern<TSquare> WithAddedAttackable(IEnumerable<TSquare> ienum, Attackable attackable)
    {
        if (attackable == Attackable.NotAttack) return WithRemovedAttackable(ienum);
        var squares = _squares;
        var attackables = _attackables;
        var forChange = ienum.Where(s => !s.IsZero && squares.ContainsKey(s)).ToHashSet();
        forChange.RemoveWhere(s => attackables.TryGetValue(s, out var v) && v == attackable);
        if (forChange.Count == 0) return this;
        var newAttack = attackables.SetItems(forChange.Select(s => new KeyValuePair<TSquare, Attackable>(s, attackable)));
        return new Pattern<TSquare>(_squares, newAttack, GuaranteesAtLeastOneStayable, recalculate: false);
    }

    public Pattern<TSquare> WithRemovedAttackable(IEnumerable<TSquare> ienum)
    {
        if (_attackables.Count == 0) return this;
        var attackables = _attackables;
        var forRemove = ienum.Where(s => !s.IsZero && attackables.ContainsKey(s)).ToHashSet();
        if (forRemove.Count == 0) return this;
        var newAttack = attackables.RemoveRange(forRemove);
        return new Pattern<TSquare>(_squares, newAttack, GuaranteesAtLeastOneStayable, recalculate: false);
    }

    public (ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee) DataWithAddedAttackable(IEnumerable<TSquare> ienum, Attackable attackable)
    {
        if (attackable == Attackable.NotAttack) return DataWithRemovedAttackable(ienum);
        var squares = _squares;
        var attackables = _attackables;
        var forChange = ienum.Where(s => !s.IsZero && squares.ContainsKey(s)).ToHashSet();
        forChange.RemoveWhere(s => attackables.TryGetValue(s, out var v) && v == attackable);
        if (forChange.Count == 0) return (_squares, _attackables, GuaranteesAtLeastOneStayable);
        var newAttack = _attackables.SetItems(forChange.Select(s => new KeyValuePair<TSquare, Attackable>(s, attackable)));
        return (_squares, newAttack, false);
    }

    public (ImmutableDictionary<TSquare, Stayable> squares, ImmutableDictionary<TSquare, Attackable> attackable, bool guarantee) DataWithRemovedAttackable(IEnumerable<TSquare> ienum)
    {
        if (_attackables.Count == 0) return (_squares, _attackables, GuaranteesAtLeastOneStayable);
        var attackables = _attackables;
        var forRemove = ienum.Where(s => !s.IsZero && attackables.ContainsKey(s)).ToHashSet();
        if (forRemove.Count == 0) return (_squares, attackables, GuaranteesAtLeastOneStayable);
        var newAttack = attackables.RemoveRange(forRemove);
        return (_squares, newAttack, false);
    }
    public override string ToString() => $"Паттерн с {Count} клетками, из них {CountOfStayables} стоячих";
    public Pattern<TSquare> Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;
    public ISequenceEnumerator<TSquare> GetEnumerator(TSquare start) => new Enumerable(_squares, _attackables, start, MaxDepth);

    public bool Equals(Pattern<TSquare> other)
    {
        if (_squares == null || _attackables == null) return other._squares == null && other._attackables == null;
        if (other._squares == null || other._attackables == null) return false;

        if (ReferenceEquals(_squares, other._squares) && ReferenceEquals(_attackables, other._attackables)) return true;
        if (_squares.Count != other._squares.Count) return false;
        foreach (var kv in _squares)
        {
            if (!other._squares.TryGetValue(kv.Key, out var v)) return false;
            if (!EqualityComparer<Stayable>.Default.Equals(kv.Value, v)) return false;
        }
        if (_attackables.Count != other._attackables.Count) return false;
        foreach (var kv in _attackables)
        {
            if (!other._attackables.TryGetValue(kv.Key, out var v)) return false;
            if (kv.Value != v) return false;
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
        private readonly ImmutableDictionary<TSquare, Attackable> _baseAttackable;
        private Dictionary<TSquare, Stayable>? _allPositionsCache;
        private Dictionary<TSquare, Attackable>? _attackableCache;
        private List<TSquare>? _onlyStayablesCache;

        public Enumerable(
            ImmutableDictionary<TSquare, Stayable> baseSquares,
            ImmutableDictionary<TSquare, Attackable> baseAttackable,
            TSquare startSquare,
            int maxDepth) : base(startSquare)
        {
            _baseSquares = baseSquares;
            _baseAttackable = baseAttackable;
            MaxDepth = maxDepth;
        }

        public override bool SupportsExplicitAttackable => true;
        public override int MaxDepth { get; }

        protected override Dictionary<TSquare, Stayable> CreateAllPossiblePositions
        {
            get
            {
                if (_allPositionsCache != null) return _allPositionsCache;
                var dict = new Dictionary<TSquare, Stayable> { [StartPosition] = Stayable.NotStay };
                foreach (var (relPos, stayable) in _baseSquares)
                {
                    var absPos = StartPosition.Add(relPos);
                    if (absPos.Equals(StartPosition)) dict[absPos] = Stayable.NotStay;
                    else dict[absPos] = stayable;
                }
                _allPositionsCache = dict;
                return dict;
            }
        }

        public override IReadOnlyDictionary<TSquare, Attackable> PositionsAttackable
        {
            get
            {
                if (_attackableCache != null) return _attackableCache;
                var dict = new Dictionary<TSquare, Attackable>(_baseAttackable.Count);
                foreach (var (relPos, attack) in _baseAttackable)
                    dict[StartPosition.Add(relPos)] = attack;
                _attackableCache = dict;
                return dict;
            }
        }

        protected override List<TSquare> CreateOnlyStayablesPositions
        {
            get
            {
                if (_onlyStayablesCache != null) return _onlyStayablesCache;
                _onlyStayablesCache = CreateAllPossiblePositions
                    .Where(kv => kv.Value == Stayable.Stay)
                    .Select(kv => kv.Key).ToList();
                return _onlyStayablesCache;
            }
        }

        protected override bool ProtectedCanMoveTo(IPath<TSquare> path)
        {
            var positions = path.Positions;
            if (positions.Count < 2) return false;
            if (positions.Count - 1 > MaxDepth) return false;
            var all = CreateAllPossiblePositions;
            foreach (var pos in positions)
                if (!all.ContainsKey(pos)) return false;
            return all.TryGetValue(positions[^1], out var stayable) && stayable == Stayable.Stay;
        }

        public override bool CanMoveTo(TSquare target)
            => CreateAllPossiblePositions.TryGetValue(target, out var stayable) && stayable == Stayable.Stay;

        public override IEnumerable<TSquare> GetNeighbors(TSquare square) => square.UnitNeighbors();
        public override IPath<TSquare> CombinePositionsIntoPath(TSquare[] positions) => new PathSequence<TSquare>(positions);
    }
}