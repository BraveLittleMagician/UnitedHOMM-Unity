#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class TurnManager
{
    private readonly List<IdOfPlayer> _order;
    private readonly HashSet<IdOfPlayer> _eliminated = new();
    private readonly IEventBus _eventBus;
    private int _index;

    public TurnManager(IReadOnlyList<StartingPlacement> placements, IEventBus eventBus)
    {
        if (placements == null) throw new ArgumentNullException(nameof(placements));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        _order = placements
            .Select(p => p.Definition.Owner)
            .Distinct()
            .OrderBy(p => p.IndexOfSide)
            .ThenBy(p => p.IndexOfPlayerOnSide)
            .ToList();

        if (_order.Count == 0)
            throw new InvalidOperationException("В расстановке нет ни одного игрока.");

        _index = 0;
    }

    public IdOfPlayer ActivePlayer => _order[_index];
    public IReadOnlyList<IdOfPlayer> Order => _order;
    public event Action<IdOfPlayer>? TurnStarted;

    private void NotifyTurnStarted(IdOfPlayer player)
    {
        TurnStarted?.Invoke(player);
        _eventBus.Publish(new TurnStartedEvent(player));
    }
    private int NextActiveIndex(int current)
    {
        int n = _order.Count;
        for (int step = 1; step <= n; step++)
        {
            int candidate = (current + step) % n;
            if (!_eliminated.Contains(_order[candidate]))
                return candidate;
        }
        return current;
    }

    public void Start()
    {
        NotifyTurnStarted(ActivePlayer);
    }
    public void EndTurn()
    {
        _index = NextActiveIndex(_index);
        NotifyTurnStarted(ActivePlayer);
    }
    public void Eliminate(IdOfPlayer player) => _eliminated.Add(player);
    public bool IsEliminated(IdOfPlayer player) => _eliminated.Contains(player);
}