#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class TurnManager
{
    private readonly List<IdOfPlayer> _order;
    private readonly HashSet<IdOfPlayer> _eliminated = new();
    private int _index;

    public TurnManager(IEnumerable<IdOfPlayer> players)
    {
        if (players == null) throw new ArgumentNullException(nameof(players));
        _order = players.Distinct().OrderBy(p => p.IndexOfSide).ThenBy(p => p.IndexOfPlayerOnSide).ToList();

        if (_order.Count == 0) throw new InvalidOperationException("В расстановке нет ни одного игрока.");
        _index = 0;
    }

    public IdOfPlayer ActivePlayer => _order[_index];
    public IReadOnlyList<IdOfPlayer> Order => _order;
    public event Action<IdOfPlayer>? TurnStarted;

    public void Start() => TurnStarted?.Invoke(ActivePlayer);

    public void EndTurn()
    {
        _index = NextActiveIndex(_index);
        TurnStarted?.Invoke(ActivePlayer);
    }

    public void Eliminate(IdOfPlayer player) => _eliminated.Add(player);
    public bool IsEliminated(IdOfPlayer player) => _eliminated.Contains(player);

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
}