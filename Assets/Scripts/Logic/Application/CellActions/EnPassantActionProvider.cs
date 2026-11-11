#nullable enable
using System;
using System.Collections.Generic;

public sealed class EnPassantActionProvider : ICellActionProvider
{
    private readonly IFlow _flow;

    public EnPassantActionProvider(IFlow flow)
        => _flow = flow ?? throw new ArgumentNullException(nameof(flow));

    public IReadOnlyList<CellActionOption> GetActionsForCell(IPiece actor, Square cell)
    {
        if (actor == null || actor.IsDead) return Array.Empty<CellActionOption>();
        var targets = _flow.GetAvailableEnPassantTargets(actor);
        if (targets.Count == 0) return Array.Empty<CellActionOption>();

        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (!t.VictimCell.Equals(cell)) continue;
            string desc = $"Strike {t.Victim.Name} and move to via cell";
            return new[]
            {
                new CellActionOption("En Passant", desc, -1, -1,
                    () => _flow.EnPassantAttack(actor, t)),
            };
        }
        return Array.Empty<CellActionOption>();
    }

    public IReadOnlyList<Square> GetActionCells(IPiece actor)
    {
        if (actor == null || actor.IsDead) return Array.Empty<Square>();
        var targets = _flow.GetAvailableEnPassantTargets(actor);
        if (targets.Count == 0) return Array.Empty<Square>();
        var result = new List<Square>(targets.Count);
        for (int i = 0; i < targets.Count; i++) result.Add(targets[i].VictimCell);
        return result;
    }
}