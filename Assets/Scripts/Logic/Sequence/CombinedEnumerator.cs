#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class CombinedEnumerator : ISequenceEnumerator<Square>
{
    private readonly ISequenceEnumerator<Square> _move;
    private readonly ISequenceEnumerator<Square> _attack;

    private Dictionary<Square, Stayable>? _positionsAllCache;
    private List<Square>? _stayablesCache;
    private Dictionary<Square, Attackable>? _attackableCache;

    public CombinedEnumerator(ISequenceEnumerator<Square> move, ISequenceEnumerator<Square> attack)
    {
        _move = move ?? throw new ArgumentNullException(nameof(move));
        _attack = attack ?? throw new ArgumentNullException(nameof(attack));
        StartPosition = move.StartPosition;
        MaxDepth = Math.Max(move.MaxDepth, attack.MaxDepth);
    }

    public bool SupportsExplicitAttackable => true;
    public int MaxDepth { get; }
    public Square StartPosition { get; }

    public Dictionary<Square, Stayable> PositionsAll
    {
        get
        {
            if (_positionsAllCache != null) return _positionsAllCache;
            var result = new Dictionary<Square, Stayable>(_move.PositionsAll);
            foreach (var kv in _attack.PositionsAll)
            {
                if (!result.ContainsKey(kv.Key))
                    result[kv.Key] = Stayable.NotStay;
            }
            _positionsAllCache = result;
            return result;
        }
    }

    public List<Square> PositionsStayables
    {
        get
        {
            if (_stayablesCache != null) return _stayablesCache;
            _stayablesCache = _move.PositionsStayables.ToList();
            return _stayablesCache;
        }
    }

    public IReadOnlyDictionary<Square, Attackable> PositionsAttackable
    {
        get
        {
            if (_attackableCache != null) return _attackableCache;
            var result = new Dictionary<Square, Attackable>();

            foreach (var kv in _move.PositionsAttackable)
                result[kv.Key] = kv.Value;

            if (_attack.SupportsExplicitAttackable)
            {
                foreach (var kv in _attack.PositionsAttackable)
                    if (kv.Value == Attackable.Attack)
                        result[kv.Key] = Attackable.Attack;
            }
            else
            {
                foreach (var cell in _attack.PositionsStayables)
                    result[cell] = Attackable.Attack;
            }

            _attackableCache = result;
            return result;
        }
    }

    public IEnumerable<Square> GetNeighbors(Square cell)
    {
        // Guard: расширяемся ТОЛЬКО с клеток, достижимых через move-форму.
        // Это делает attack-only клетки (например, диагонали у Pawn) терминальными:
        // в них можно прийти и атаковать, но нельзя пройти сквозь них дальше.
        // Без этого guard'а Pawn смог бы «ехать» по диагонали, что ломает
        // «пешка ходит строго вперёд».
        if (!_move.PositionsAll.ContainsKey(cell)) yield break;

        foreach (var n in _move.GetNeighbors(cell)) yield return n;
        foreach (var n in _attack.GetNeighbors(cell)) yield return n;
    }

    public IPath<Square> CombinePositionsIntoPath(Square[] positions)
        => _move.CombinePositionsIntoPath(positions);

    public bool CanMoveTo(Square target) => _move.CanMoveTo(target);

    public bool CanMoveTo(IPath<Square> path) => _move.CanMoveTo(path);
}