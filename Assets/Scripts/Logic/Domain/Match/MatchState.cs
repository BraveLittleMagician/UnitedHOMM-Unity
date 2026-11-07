#nullable enable
using System;
using System.Collections.Generic;

public sealed class MatchState
{
    private readonly HashSet<IdOfPlayer> _eliminated = new();

    public bool IsOver { get; private set; }
    public IdOfPlayer? Winner { get; private set; }
    public IReadOnlyCollection<IdOfPlayer> EliminatedPlayers => _eliminated;

    public event Action? Changed;

    public bool IsEliminated(IdOfPlayer player) => _eliminated.Contains(player);

    public void MarkEliminated(IdOfPlayer player)
    {
        if (!_eliminated.Add(player)) return;
        Changed?.Invoke();
    }

    public void SetGameOver(IdOfPlayer? winner)
    {
        if (IsOver) return;
        IsOver = true;
        Winner = winner;
        Changed?.Invoke();
    }
}