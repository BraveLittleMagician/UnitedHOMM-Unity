#nullable enable

using System;
using System.Collections.Generic;

public sealed class MatchEndChecker
{
    private readonly IRegistry _registry;
    private readonly TurnManager _turnManager;
    private readonly MatchState _matchState;

    public MatchEndChecker(IRegistry registry, TurnManager turnManager, MatchState matchState)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
        _matchState = matchState ?? throw new ArgumentNullException(nameof(matchState));
    }

    public bool CheckAfterCommit()
    {
        if (_matchState.IsOver) return true;

        foreach (var player in _turnManager.Order)
        {
            if (_matchState.IsEliminated(player)) continue;
            if (HasLivePieces(player)) continue;
            _matchState.MarkEliminated(player);
            _turnManager.Eliminate(player);
        }

        var survivors = new List<IdOfPlayer>();
        foreach (var player in _turnManager.Order)
            if (!_matchState.IsEliminated(player))
                survivors.Add(player);

        if (survivors.Count == 1)
        {
            _matchState.SetGameOver(survivors[0]);
            return true;
        }
        if (survivors.Count == 0)
        {
            _matchState.SetGameOver(null);
            return true;
        }
        return false;
    }

    private bool HasLivePieces(IdOfPlayer player)
    {
        foreach (var piece in _registry.AllPieces)
            if (piece.Owner.Equals(player) && !piece.IsDead)
                return true;
        return false;
    }
}
