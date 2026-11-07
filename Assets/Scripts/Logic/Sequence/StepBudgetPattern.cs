#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public readonly struct StepBudgetPattern : ISequence<Square, StepBudgetPattern>
{
    private readonly ImmutableHashSet<Square> _stayCells;
    private readonly int _maxSteps;
    private readonly MultipleAxes _activeAxes;
    private readonly ImmutableDictionary<Square, Stayable> _relativePositions;
    private readonly ImmutableDictionary<Square, Attackable> _attackable;
    private readonly int _cachedHash;

    private StepBudgetPattern(ImmutableHashSet<Square> stayCells, int maxSteps, MultipleAxes activeAxes, ImmutableDictionary<Square, Stayable> relativePositions, ImmutableDictionary<Square, Attackable> attackable, bool guaranteesAtLeastOneStayable, int cachedHash)
    {
        _stayCells = stayCells;
        _maxSteps = maxSteps;
        _activeAxes = activeAxes;
        _relativePositions = relativePositions;
        _attackable = attackable;
        GuaranteesAtLeastOneStayable = guaranteesAtLeastOneStayable;
        _cachedHash = cachedHash;
    }
    public StepBudgetPattern(IReadOnlyCollection<Square> stayCells, int maxSteps) : this(stayCells, maxSteps, MultipleAxes.Three) { }
    public StepBudgetPattern( IReadOnlyCollection<Square> stayCells, int maxSteps, MultipleAxes activeAxes)
    {
        if (stayCells == null) throw new ArgumentNullException(nameof(stayCells));
        if (maxSteps <= 0) throw new ArgumentOutOfRangeException(nameof(maxSteps), "maxSteps must be positive.");
        if (stayCells.Contains(Square.Zero)) throw new ArgumentException("Stay cells must not contain the origin.", nameof(stayCells));
        if (activeAxes == MultipleAxes.None) throw new ArgumentException("activeAxes must include at least one axis.", nameof(activeAxes));

        foreach (var cell in stayCells)
        {
            if (cell.Y != 0 && !activeAxes.HasFlag(MultipleAxes.Two)) throw new ArgumentException($"Stay cell {cell} uses Y axis, but activeAxes={activeAxes}.", nameof(stayCells));
            if (cell.Z != 0 && !activeAxes.HasFlag(MultipleAxes.Three)) throw new ArgumentException($"Stay cell {cell} uses Z axis, but activeAxes={activeAxes}.", nameof(stayCells));
            if (cell.W != 0 && !activeAxes.HasFlag(MultipleAxes.Four)) throw new ArgumentException($"Stay cell {cell} uses W axis, but activeAxes={activeAxes}.", nameof(stayCells));
        }

        _stayCells = stayCells.ToImmutableHashSet();
        _maxSteps = maxSteps;
        _activeAxes = activeAxes;

        var offsets = BuildOffsets(activeAxes);
        var builder = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        builder[Square.Zero] = Stayable.NotStay;

        var visited = new HashSet<Square> { Square.Zero };
        var queue = new Queue<(Square pos, int depth)>();
        queue.Enqueue((Square.Zero, 0));

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (depth >= maxSteps) continue;
            foreach (var offset in offsets)
            {
                var neighbor = current.Add(offset);
                if (visited.Contains(neighbor)) continue;
                visited.Add(neighbor);
                var stayable = _stayCells.Contains(neighbor) ? Stayable.Stay : Stayable.NotStay;
                builder[neighbor] = stayable;
                queue.Enqueue((neighbor, depth + 1));
            }
        }

        _relativePositions = builder.ToImmutable();
        _attackable = ImmutableDictionary<Square, Attackable>.Empty;

        bool hasStay = false;
        foreach (var s in _relativePositions.Values)
            if (s == Stayable.Stay) { hasStay = true; break; }
        GuaranteesAtLeastOneStayable = hasStay;

        _cachedHash = ComputeHash(_relativePositions, _attackable, GuaranteesAtLeastOneStayable, maxSteps, activeAxes);
    }

    private static int ComputeHash(ImmutableDictionary<Square, Stayable> relativePositions, ImmutableDictionary<Square, Attackable> attackable, bool guarantee, int maxSteps, MultipleAxes activeAxes)
    {
        var hash = new HashCode();
        hash.Add(guarantee);
        hash.Add(maxSteps);
        hash.Add((int)activeAxes);
        foreach (var kv in relativePositions)
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
    private static IReadOnlyList<Square> BuildOffsets(MultipleAxes axes)
    {
        int dyRange = axes.HasFlag(MultipleAxes.Two) ? 1 : 0;
        int dzRange = axes.HasFlag(MultipleAxes.Three) ? 1 : 0;
        int dwRange = axes.HasFlag(MultipleAxes.Four) ? 1 : 0;

        var list = new List<Square>();
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -dyRange; dy <= dyRange; dy++)
                for (int dz = -dzRange; dz <= dzRange; dz++)
                    for (int dw = -dwRange; dw <= dwRange; dw++)
                    {
                        if (dx == 0 && dy == 0 && dz == 0 && dw == 0) continue;
                        list.Add(new Square { X = dx, Y = dy, Z = dz, W = dw });
                    }
        return list;
    }

    public bool GuaranteesAtLeastOneStayable { get; }
    public int MaxSteps => _maxSteps;
    public MultipleAxes ActiveAxes => _activeAxes;
    public IReadOnlyCollection<Square> StayCells => _stayCells;

    public ISequenceEnumerator<Square> GetEnumerator(Square start) => new Enumerable(_relativePositions, _attackable, start, _maxSteps);

    public StepBudgetPattern Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;

    public bool Equals(StepBudgetPattern other)
    {
        if (_relativePositions == null || _attackable == null) return other._relativePositions == null && other._attackable == null;
        if (other._relativePositions == null || other._attackable == null) return false;

        if (_maxSteps != other._maxSteps) return false;
        if (_activeAxes != other._activeAxes) return false;
        if (_relativePositions.Count != other._relativePositions.Count) return false;
        foreach (var kv in _relativePositions)
        {
            if (!other._relativePositions.TryGetValue(kv.Key, out var v)) return false;
            if (v != kv.Value) return false;
        }
        if (_attackable.Count != other._attackable.Count) return false;
        foreach (var kv in _attackable)
        {
            if (!other._attackable.TryGetValue(kv.Key, out var v)) return false;
            if (kv.Value != v) return false;
        }
        return true;
    }
    public override bool Equals(object? obj) => obj is StepBudgetPattern p && Equals(p);
    public override int GetHashCode() => _cachedHash;
    public static bool operator ==(StepBudgetPattern a, StepBudgetPattern b) => a.Equals(b);
    public static bool operator !=(StepBudgetPattern a, StepBudgetPattern b) => !(a == b);

    public StepBudgetPattern WithAddedAttackable(IEnumerable<Square> ienum, Attackable attackable)
    {
        if (attackable == Attackable.NotAttack) return WithRemovedAttackable(ienum);
        var relative = _relativePositions;
        var attackables = _attackable;
        var forChange = ienum.Where(s => !s.IsZero && relative.ContainsKey(s)).ToHashSet();
        forChange.RemoveWhere(s => attackables.TryGetValue(s, out var v) && v == attackable);
        if (forChange.Count == 0) return this;
        var newAttack = attackables.SetItems(forChange.Select(s => new KeyValuePair<Square, Attackable>(s, attackable)));
        return new StepBudgetPattern(
            _stayCells, _maxSteps, _activeAxes,
            _relativePositions, newAttack,
            GuaranteesAtLeastOneStayable,
            ComputeHash(_relativePositions, newAttack, GuaranteesAtLeastOneStayable, _maxSteps, _activeAxes));
    }
    public StepBudgetPattern WithRemovedAttackable(IEnumerable<Square> ienum)
    {
        if (_attackable.Count == 0) return this;
        var attackables = _attackable;
        var forRemove = ienum.Where(s => !s.IsZero && attackables.ContainsKey(s)).ToHashSet();
        if (forRemove.Count == 0) return this;
        var newAttack = attackables.RemoveRange(forRemove);
        return new StepBudgetPattern(
            _stayCells, _maxSteps, _activeAxes,
            _relativePositions, newAttack,
            GuaranteesAtLeastOneStayable,
            ComputeHash(_relativePositions, newAttack, GuaranteesAtLeastOneStayable, _maxSteps, _activeAxes));
    }
    public (ImmutableDictionary<Square, Stayable> relativePositions, ImmutableDictionary<Square, Attackable> attackable, bool guarantee) DataWithAddedAttackable(IEnumerable<Square> ienum, Attackable attackable)
    {
        if (attackable == Attackable.NotAttack) return DataWithRemovedAttackable(ienum);
        var relative = _relativePositions;
        var attackables = _attackable;
        var forChange = ienum.Where(s => !s.IsZero && relative.ContainsKey(s)).ToHashSet();
        forChange.RemoveWhere(s => attackables.TryGetValue(s, out var v) && v == attackable);
        if (forChange.Count == 0) return (_relativePositions, _attackable, GuaranteesAtLeastOneStayable);
        var newAttack = attackables.SetItems(forChange.Select(s => new KeyValuePair<Square, Attackable>(s, attackable)));
        return (_relativePositions, newAttack, GuaranteesAtLeastOneStayable);
    }
    public (ImmutableDictionary<Square, Stayable> relativePositions, ImmutableDictionary<Square, Attackable> attackable, bool guarantee) DataWithRemovedAttackable(IEnumerable<Square> ienum)
    {
        if (_attackable.Count == 0) return (_relativePositions, _attackable, GuaranteesAtLeastOneStayable);
        var attackables = _attackable;
        var forRemove = ienum.Where(s => !s.IsZero && attackables.ContainsKey(s)).ToHashSet();
        if (forRemove.Count == 0) return (_relativePositions, attackables, GuaranteesAtLeastOneStayable);
        var newAttack = attackables.RemoveRange(forRemove);
        return (_relativePositions, newAttack, GuaranteesAtLeastOneStayable);
    }

    private sealed class Enumerable : SequenceEnumerable<Square>
    {
        private readonly int _maxDepth;
        private readonly ImmutableDictionary<Square, Stayable> _relativePositions;
        private readonly ImmutableDictionary<Square, Attackable> _attackable;
        private List<Square>? _stayablesCache;
        private Dictionary<Square, Stayable>? _allPositionsCache;
        private Dictionary<Square, Attackable>? _attackableCache;

        public Enumerable(ImmutableDictionary<Square, Stayable> relativePositions, ImmutableDictionary<Square, Attackable> attackable, Square start, int maxDepth) : base(start)
        {
            _relativePositions = relativePositions;
            _attackable = attackable;
            _maxDepth = maxDepth;
        }

        public override bool SupportsExplicitAttackable => true;
        public override int MaxDepth => _maxDepth;

        protected override List<Square> CreateOnlyStayablesPositions
        {
            get
            {
                if (_stayablesCache != null) return _stayablesCache;
                _stayablesCache = CreateAllPossiblePositions
                    .Where(kv => kv.Value == Stayable.Stay)
                    .Select(kv => kv.Key)
                    .ToList();
                return _stayablesCache;
            }
        }

        protected override Dictionary<Square, Stayable> CreateAllPossiblePositions
        {
            get
            {
                if (_allPositionsCache != null) return _allPositionsCache;
                var dict = new Dictionary<Square, Stayable>(_relativePositions.Count);
                foreach (var kv in _relativePositions)
                    dict[StartPosition + kv.Key] = kv.Value;
                _allPositionsCache = dict;
                return dict;
            }
        }

        public override IReadOnlyDictionary<Square, Attackable> PositionsAttackable
        {
            get
            {
                if (_attackableCache != null) return _attackableCache;
                var dict = new Dictionary<Square, Attackable>(_attackable.Count);
                foreach (var (rel, attack) in _attackable)
                    dict[StartPosition + rel] = attack;
                _attackableCache = dict;
                return dict;
            }
        }

        protected override bool ProtectedCanMoveTo(IPath<Square> path)
        {
            var positions = path.Positions;
            if (positions.Count < 2) return false;
            if (positions.Count - 1 > _maxDepth) return false;
            if (!positions[0].Equals(StartPosition)) return false;
            for (int i = 0; i < positions.Count - 1; i++)
            {
                if (!positions[i].IsAdjacent(positions[i + 1])) return false;
            }
            var all = CreateAllPossiblePositions;
            foreach (var p in positions)
                if (!all.ContainsKey(p)) return false;
            var last = positions[^1];
            return all.TryGetValue(last, out var flag) && flag == Stayable.Stay;
        }

        public override IEnumerable<Square> GetNeighbors(Square cell)
        {
            var all = CreateAllPossiblePositions;
            foreach (var n in cell.UnitNeighbors())
                if (all.ContainsKey(n))
                    yield return n;
        }

        public override bool CanMoveTo(Square target)
        {
            return CreateAllPossiblePositions.TryGetValue(target, out var flag)
                && flag == Stayable.Stay;
        }

        public override IPath<Square> CombinePositionsIntoPath(Square[] positions) => new PathSequence<Square>(positions);
    }
}