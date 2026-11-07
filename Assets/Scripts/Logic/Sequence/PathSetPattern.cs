#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public readonly struct PathSetPattern : ISequence<Square, PathSetPattern>
{
    private readonly ImmutableDictionary<Square, Stayable> _squares;
    private readonly ImmutableDictionary<Square, Attackable> _attackable;
    private readonly ImmutableDictionary<Square, ImmutableHashSet<Square>> _nextMap;
    private readonly int _cachedHash;
    
    private PathSetPattern(ImmutableDictionary<Square, Stayable> squares, ImmutableDictionary<Square, ImmutableHashSet<Square>> nextMap, ImmutableDictionary<Square, Attackable> attackable, bool guaranteesAtLeastOneStayable, int cachedHash)
    {
        _squares = squares;
        _nextMap = nextMap;
        _attackable = attackable;
        GuaranteesAtLeastOneStayable = guaranteesAtLeastOneStayable;
        _cachedHash = cachedHash;
    }

    public PathSetPattern(IReadOnlyList<MoveRule> rules, int maxRepeat)
    {
        if (rules == null || rules.Count == 0) throw new ArgumentException("At least one rule required.", nameof(rules));
        if (maxRepeat <= 0) throw new ArgumentOutOfRangeException(nameof(maxRepeat), "maxRepeat must be positive.");

        var squares = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        var nextMap = ImmutableDictionary.CreateBuilder<Square, ImmutableHashSet<Square>>();
        var attackable = ImmutableDictionary.CreateBuilder<Square, Attackable>();
        squares[Square.Zero] = Stayable.NotStay;

        foreach (var rule in rules)
        {
            var current = Square.Zero;
            foreach (var step in rule.Steps)
            {
                int count = step.Repeat == -1 ? maxRepeat : step.Repeat;
                for (int i = 0; i < count; i++)
                {
                    var next = current + step.Offset;
                    if (!squares.TryGetValue(next, out var existing) || step.Stayable == Stayable.Stay)
                        squares[next] = step.Stayable;
                    if (step.Attackable == Attackable.Attack)
                        attackable[next] = Attackable.Attack;
                    if (!nextMap.TryGetValue(current, out var set))
                    {
                        set = ImmutableHashSet<Square>.Empty;
                        nextMap[current] = set;
                    }
                    nextMap[current] = set.Add(next);
                    current = next;
                }
            }
        }

        _squares = squares.ToImmutable();
        _nextMap = nextMap.ToImmutable();
        _attackable = attackable.ToImmutable();

        GuaranteesAtLeastOneStayable = _squares.Values.Any(s => s == Stayable.Stay);
        _cachedHash = ComputeHash(_squares, _nextMap, _attackable, GuaranteesAtLeastOneStayable);
    }

    public bool GuaranteesAtLeastOneStayable { get; }

    private static int ComputeHash(ImmutableDictionary<Square, Stayable> squares, ImmutableDictionary<Square, ImmutableHashSet<Square>> nextMap, ImmutableDictionary<Square, Attackable> attackable, bool guarantee)
    {
        var hash = new HashCode();
        hash.Add(guarantee);
        foreach (var kv in squares.OrderBy(kv => kv.Key))
        {
            hash.Add(kv.Key);
            hash.Add(kv.Value);
        }
        foreach (var kv in nextMap.OrderBy(kv => kv.Key))
        {
            hash.Add(kv.Key);
            foreach (var next in kv.Value.OrderBy(s => s))
                hash.Add(next);
        }
        foreach (var kv in attackable.OrderBy(kv => kv.Key))
        {
            hash.Add(kv.Key);
            hash.Add(kv.Value);
        }
        return hash.ToHashCode();
    }
    public ISequenceEnumerator<Square> GetEnumerator(Square start) => new Enumerable(_squares, _nextMap, _attackable, start);

    public PathSetPattern Copy() => this;
    ISequence ICopyable<ISequence>.Copy() => this;

    public PathSetPattern WithAddedAttackable(IEnumerable<Square> ienum, Attackable attackable)
    {
        if (attackable == Attackable.NotAttack) return WithRemovedAttackable(ienum);
        var squares = _squares;
        var attackables = _attackable;
        var forChange = ienum.Where(s => !s.IsZero && squares.ContainsKey(s)).ToHashSet();
        forChange.RemoveWhere(s => attackables.TryGetValue(s, out var v) && v == attackable);
        if (forChange.Count == 0) return this;
        var newAttack = attackables.SetItems(forChange.Select(s => new KeyValuePair<Square, Attackable>(s, attackable)));
        return new PathSetPattern(
            _squares, _nextMap, newAttack,
            GuaranteesAtLeastOneStayable,
            ComputeHash(_squares, _nextMap, newAttack, GuaranteesAtLeastOneStayable));
    }
    public PathSetPattern WithRemovedAttackable(IEnumerable<Square> ienum)
    {
        if (_attackable.Count == 0) return this;
        var attackables = _attackable;
        var forRemove = ienum.Where(s => !s.IsZero && attackables.ContainsKey(s)).ToHashSet();
        if (forRemove.Count == 0) return this;
        var newAttack = attackables.RemoveRange(forRemove);
        return new PathSetPattern(
            _squares, _nextMap, newAttack,
            GuaranteesAtLeastOneStayable,
            ComputeHash(_squares, _nextMap, newAttack, GuaranteesAtLeastOneStayable));
    }
    public (ImmutableDictionary<Square, Stayable> squares, ImmutableDictionary<Square, ImmutableHashSet<Square>> nextMap, ImmutableDictionary<Square, Attackable> attackable, bool guaranteesAtLeastOneStayable) DataWithAddedAttackable(IEnumerable<Square> ienum, Attackable attackable)
    {
        if (attackable == Attackable.NotAttack) return DataWithRemovedAttackable(ienum);
        var squares = _squares;
        var attackables = _attackable;
        var forChange = ienum.Where(s => !s.IsZero && squares.ContainsKey(s)).ToHashSet();
        forChange.RemoveWhere(s => attackables.TryGetValue(s, out var v) && v == attackable);
        if (forChange.Count == 0) return (_squares, _nextMap, _attackable, GuaranteesAtLeastOneStayable);
        var newAttack = attackables.SetItems(forChange.Select(s => new KeyValuePair<Square, Attackable>(s, attackable)));
        return (_squares, _nextMap, newAttack, GuaranteesAtLeastOneStayable);
    }
    public (ImmutableDictionary<Square, Stayable> squares, ImmutableDictionary<Square, ImmutableHashSet<Square>> nextMap, ImmutableDictionary<Square, Attackable> attackable, bool guaranteesAtLeastOneStayable) DataWithRemovedAttackable(IEnumerable<Square> ienum)
    {
        if (_attackable.Count == 0) return (_squares, _nextMap, _attackable, GuaranteesAtLeastOneStayable);
        var attackables = _attackable;
        var forRemove = ienum.Where(s => !s.IsZero && attackables.ContainsKey(s)).ToHashSet();
        if (forRemove.Count == 0) return (_squares, _nextMap, attackables, GuaranteesAtLeastOneStayable);
        var newAttack = attackables.RemoveRange(forRemove);
        return (_squares, _nextMap, newAttack, GuaranteesAtLeastOneStayable);
    }

    public bool Equals(PathSetPattern other)
    {
        if (_squares == null || _nextMap == null || _attackable == null) return other._squares == null && other._nextMap == null && other._attackable == null;
        if (other._squares == null || other._nextMap == null || other._attackable == null) return false;

        if (_squares.Count != other._squares.Count) return false;
        foreach (var kv in _squares)
        {
            if (!other._squares.TryGetValue(kv.Key, out var v)) return false;
            if (v != kv.Value) return false;
        }
        if (_nextMap.Count != other._nextMap.Count) return false;
        foreach (var kv in _nextMap)
        {
            if (!other._nextMap.TryGetValue(kv.Key, out var otherSet)) return false;
            if (!kv.Value.SetEquals(otherSet)) return false;
        }
        if (_attackable.Count != other._attackable.Count) return false;
        foreach (var kv in _attackable)
        {
            if (!other._attackable.TryGetValue(kv.Key, out var v)) return false;
            if (kv.Value != v) return false;
        }
        return true;
    }
    public override bool Equals(object? obj) => obj is PathSetPattern p && Equals(p);
    public override int GetHashCode() => _cachedHash;
    public static bool operator ==(PathSetPattern a, PathSetPattern b) => a.Equals(b);
    public static bool operator !=(PathSetPattern a, PathSetPattern b) => !(a == b);

    private sealed class Enumerable : SequenceEnumerable<Square>
    {
        private readonly ImmutableDictionary<Square, Attackable> _attackable;
        private readonly ImmutableDictionary<Square, Stayable> _squares;
        private readonly ImmutableDictionary<Square, ImmutableHashSet<Square>> _nextMap;
        private Dictionary<Square, Stayable>? _allPositionsCache;
        private Dictionary<Square, Attackable>? _attackableCache;
        private List<Square>? _stayablesCache;

        public Enumerable(ImmutableDictionary<Square, Stayable> squares, ImmutableDictionary<Square, ImmutableHashSet<Square>> nextMap, ImmutableDictionary<Square, Attackable> attackable, Square start) : base(start)
        {
            _squares = squares;
            _nextMap = nextMap;
            _attackable = attackable;
        }

        protected override Dictionary<Square, Stayable> CreateAllPossiblePositions
        {
            get
            {
                if (_allPositionsCache != null) return _allPositionsCache;
                var dict = new Dictionary<Square, Stayable>(_squares.Count);
                dict[StartPosition] = Stayable.NotStay;
                foreach (var kv in _squares)
                {
                    if (kv.Key.IsZero) continue;
                    dict[StartPosition + kv.Key] = kv.Value;
                }
                _allPositionsCache = dict;
                return dict;
            }
        }
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
        public override bool SupportsExplicitAttackable => true;

        public override IEnumerable<Square> GetNeighbors(Square cell)
        {
            var relative = cell - StartPosition;
            if (!_nextMap.TryGetValue(relative, out var nexts)) yield break;
            foreach (var n in nexts)
                yield return StartPosition + n;
        }

        public override bool CanMoveTo(Square target)
        {
            var relative = target - StartPosition;
            return _squares.TryGetValue(relative, out var flag) && flag == Stayable.Stay;
        }

        protected override bool ProtectedCanMoveTo(IPath<Square> path)
        {
            var positions = path.Positions;
            if (positions.Count < 2) return false;
            if (!positions[0].Equals(StartPosition)) return false;

            for (int i = 0; i < positions.Count - 1; i++)
            {
                var from = positions[i] - StartPosition;
                var to = positions[i + 1] - StartPosition;
                if (!_nextMap.TryGetValue(from, out var nexts)) return false;
                if (!nexts.Contains(to)) return false;
            }

            var last = positions[^1] - StartPosition;
            return _squares.TryGetValue(last, out var lastFlag) && lastFlag == Stayable.Stay;
        }

        public override IPath<Square> CombinePositionsIntoPath(Square[] positions)
            => new PathSequence<Square>(positions);
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
    }
}