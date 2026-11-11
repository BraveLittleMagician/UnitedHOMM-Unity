#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;

public abstract class ReactiveActionService<TRecord, TTarget> : IDisposable where TTarget : struct
{
    protected readonly IRegistry Registry;
    protected readonly TurnManager TurnManager;
    protected readonly IEventBus EventBus;
    protected readonly ModifierHost ModifierHost;

    private readonly List<TRecord> _records = new();
    private bool _disposed;

    protected ReactiveActionService(IRegistry registry, TurnManager turnManager, IEventBus eventBus, ModifierHost modifierHost)
    {
        Registry = registry ?? throw new ArgumentNullException(nameof(registry));
        TurnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        ModifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));

        EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        RegisterHandlers();
    }

    private void OnTurnStarted(TurnStartedEvent e)
    {
        for (int i = _records.Count - 1; i >= 0; i--)
        {
            if (!GetValidFor(_records[i]).Equals(e.Player))
                _records.RemoveAt(i);
        }
    }

    protected void AddRecord(TRecord record) => _records.Add(record);
    protected abstract void RegisterHandlers();
    protected abstract void UnregisterHandlers();
    protected abstract bool PassesBaseCheck(IPiece attacker);
    protected abstract bool IsValidRecord(TRecord record, IPiece attacker);
    protected abstract TTarget BuildTarget(TRecord record, IPiece attacker);
    protected abstract IdOfPlayer GetValidFor(TRecord record);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        UnregisterHandlers();
        _records.Clear();
    }
    public IReadOnlyList<TTarget> GetAvailableTargets(IPiece attacker)
    {
        var result = new List<TTarget>();
        if (attacker == null || attacker.IsDead) return result;
        if (!PassesBaseCheck(attacker)) return result;

        for (int i = 0; i < _records.Count; i++)
        {
            var record = _records[i];
            if (!GetValidFor(record).Equals(attacker.Owner)) continue;
            if (!IsValidRecord(record, attacker)) continue;
            result.Add(BuildTarget(record, attacker));
        }
        return result;
    }
}