#nullable enable
using System;
using System.Collections.Generic;

public sealed class CellActionHandler
{
    private readonly IFlow _flow;
    private readonly PendingModalManager _modalManager;
    private readonly MatchState _matchState;
    private readonly IReadOnlyList<ICellActionProvider> _providers;
    private readonly ILogger _logger;

    public event Action? StateChanged;

    public CellActionHandler(CellActionHandlerContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _flow = context.Flow;
        _modalManager = context.PendingModalManager;
        _matchState = context.MatchState;
        _providers = context.CellActionProviders ?? Array.Empty<ICellActionProvider>();
        _logger = context.Logger;
    }

    public void HandleActionCellClick(IPiece actor, Square cell)
    {
        if (_matchState.IsOver) return;
        if (_modalManager.HasPending) return;
        if (actor == null || actor.IsDead) return;

        var actions = CollectActions(actor, cell);
        if (actions.Count == 0) return;

        if (actions.Count == 1)
        {
            ExecuteAction(actions[0]);
            return;
        }

        _modalManager.Set(new PendingChoice(actor, cell, actions));
    }

    public void HandleChoiceSelection(int index)
    {
        if (_matchState.IsOver) return;
        var pending = _modalManager.CurrentChoice;
        if (pending == null) return;
        if (index < 0 || index >= pending.Options.Count) return;

        var option = pending.Options[index];
        _modalManager.Clear();
        ExecuteAction(option);
    }

    public void CancelChoice()
    {
        if (_modalManager.CurrentChoice != null) _modalManager.Clear();
    }

    private List<CellActionOption> CollectActions(IPiece actor, Square cell)
    {
        var actions = new List<CellActionOption>();
        for (int i = 0; i < _providers.Count; i++)
        {
            var opts = _providers[i].GetActionsForCell(actor, cell);
            for (int j = 0; j < opts.Count; j++)
                actions.Add(opts[j]);
        }
        return actions;
    }

    private void ExecuteAction(CellActionOption option)
    {
        var result = option.Execute();
        if (!result.IsSuccess)
        {
            _logger.LogWarning($"[CELL-ACTION] '{option.Label}' failed: {result.Error}");
            return;
        }
        StateChanged?.Invoke();
    }
}