#nullable enable

using System;
using System.Collections.Generic;

public sealed class ActionPlan
{
    private readonly struct Entry
    {
        public IGameAction Action { get; }
        public WorldSnapshot Before { get; }
        public Entry(IGameAction action, WorldSnapshot before)
        {
            Action = action;
            Before = before;
        }
    }

    private readonly PieceRegistry _registry;
    private readonly IHouse _house;
    private readonly TurnManager _turnManager;
    private readonly Action<IPiece?>? _syncAfterUndo;
    private readonly Func<bool>? _onCommitted;
    private readonly List<Entry> _stack = new();

    public ActionPlan(
        PieceRegistry registry,
        IHouse house,
        TurnManager turnManager,
        Action<IPiece?>? syncAfterUndo = null,
        Func<bool>? onCommitted = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _house = house ?? throw new ArgumentNullException(nameof(house));
        _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
        _syncAfterUndo = syncAfterUndo;
        _onCommitted = onCommitted;
    }

    public int Count => _stack.Count;
    public bool IsEmpty => _stack.Count == 0;

    public bool TryAdd(IGameAction action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (action.Owner != _turnManager.ActivePlayer) return false;
        if (!action.CanExecute()) return false;

        var before = WorldSnapshot.Capture(_registry, _house);

        try
        {
            action.Execute();
        }
        catch
        {
            // Bug-fix P2: при провале Execute откатываем до action.
            before.Restore(_registry, _house);
            throw;
        }

        _stack.Add(new Entry(action, before));
        return true;
    }

    public bool Undo()
    {
        if (_stack.Count == 0) return false;
        var last = _stack[^1];
        _stack.RemoveAt(_stack.Count - 1);
        last.Before.Restore(_registry, _house);
        _syncAfterUndo?.Invoke(last.Action.FocusPiece);
        return true;
    }

    public bool UndoAll()
    {
        if (_stack.Count == 0) return false;
        var focus = _stack[^1].Action.FocusPiece;
        _stack[0].Before.Restore(_registry, _house);
        _stack.Clear();
        _syncAfterUndo?.Invoke(focus);
        return true;
    }

    public void Commit()
    {
        _stack.Clear();
        bool isOver = _onCommitted?.Invoke() ?? false;
        if (isOver) return;
        _turnManager.EndTurn();
    }
}