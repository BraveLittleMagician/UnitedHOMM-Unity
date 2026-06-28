#nullable enable


using System;
using System.Collections.Generic;
using System.Linq;

public sealed class AbilityService : IAbilityService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly HashSet<IPiece> _activePieces = new();
    private bool _disposed;

    public AbilityService(IEventBus eventBus, ILogger logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ActivateAbilities(IPiece piece)
    {
        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        if (_activePieces.Contains(piece))
        {
            _logger.LogWarning($"Способности фигуры {piece} уже активированы");
            return;
        }

        foreach (var ability in piece.Abilities)
        {
            ability.Activate(piece);
            _logger.LogDebug($"Способность {ability.GetType().Name} активирована для {piece}");
        }

        _activePieces.Add(piece);
    }
    public void DeactivateAbilities(IPiece piece)
    {
        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        if (!_activePieces.Contains(piece))
        {
            _logger.LogWarning($"Способности фигуры {piece} уже деактивированы");
            return;
        }

        foreach (var ability in piece.Abilities)
        {
            ability.Deactivate();
            _logger.LogDebug($"Способность {ability.GetType().Name} деактивирована для {piece}");
        }

        _activePieces.Remove(piece);
    }
    public void ActivateAbilitiesByTrigger(IPiece piece, TriggerType trigger)
    {
        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        foreach (var ability in piece.Abilities)
        {
            if (ability.TriggerType == trigger)
            {
                ability.Activate(piece);
                _logger.LogDebug($"Способность {ability.GetType().Name} (триггер {trigger}) активирована для {piece}");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var piece in _activePieces.ToArray()) DeactivateAbilities(piece);
        _activePieces.Clear();
        _logger.Log("AbilityService уничтожен");
    }
}