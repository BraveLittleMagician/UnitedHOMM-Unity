#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class CombinedEnumerator<TPosition> : ISequenceEnumerator<TPosition>
    where TPosition : struct
{
    private readonly ISequenceEnumerator<TPosition> _move;
    private readonly ISequenceEnumerator<TPosition> _attack;

    private Dictionary<TPosition, Stayable>? _positionsAllCache;
    private List<TPosition>? _stayablesCache;
    private Dictionary<TPosition, Attackable>? _attackableCache;

    public CombinedEnumerator(ISequenceEnumerator<TPosition> move, ISequenceEnumerator<TPosition> attack)
    {
        _move = move ?? throw new ArgumentNullException(nameof(move));
        _attack = attack ?? throw new ArgumentNullException(nameof(attack));
        StartPosition = move.StartPosition;
        MaxDepth = Math.Max(move.MaxDepth, attack.MaxDepth);
    }

    public bool SupportsExplicitAttackable => true;
    public int MaxDepth { get; }
    public TPosition StartPosition { get; }
    public bool CanMoveTo(TPosition target) => _move.CanMoveTo(target);
    public bool CanMoveTo(IPath<TPosition> path) => _move.CanMoveTo(path);

    public Dictionary<TPosition, Stayable> PositionsAll
    {
        get
        {
            if (_positionsAllCache != null) return _positionsAllCache;
            var result = new Dictionary<TPosition, Stayable>(_move.PositionsAll);
            foreach (var kv in _attack.PositionsAll)
            {
                if (!result.ContainsKey(kv.Key))
                    result[kv.Key] = Stayable.NotStay;
            }
            _positionsAllCache = result;
            return result;
        }
    }
    public List<TPosition> PositionsStayables
    {
        get
        {
            if (_stayablesCache != null) return _stayablesCache;
            _stayablesCache = _move.PositionsStayables.ToList();
            return _stayablesCache;
        }
    }
    public IReadOnlyDictionary<TPosition, Attackable> PositionsAttackable
    {
        get
        {
            if (_attackableCache != null) return _attackableCache;
            var result = new Dictionary<TPosition, Attackable>();
            foreach (var kv in _move.PositionsAttackable) result[kv.Key] = kv.Value;
            foreach (var cell in _attack.PositionsStayables) result[cell] = Attackable.Attack;
            _attackableCache = result;
            return result;
        }
    }
    public IEnumerable<TPosition> GetNeighbors(TPosition cell)
    {
        // Guard: расширяемся ТОЛЬКО с клеток, достижимых через move-форму.
        // Это делает attack-only клетки (например, диагонали у Pawn) терминальными:
        // в них можно прийти и атаковать, но нельзя пройти сквозь них дальше.
        if (!_move.PositionsAll.ContainsKey(cell)) yield break;

        foreach (var n in _move.GetNeighbors(cell)) yield return n;
        foreach (var n in _attack.GetNeighbors(cell)) yield return n;
    }
    public IPath<TPosition> CombinePositionsIntoPath(TPosition[] positions) => _move.CombinePositionsIntoPath(positions);
}