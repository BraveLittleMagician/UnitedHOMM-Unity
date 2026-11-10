#nullable enable

using System;
using UnityEngine;

public sealed class TargetedModifierAction : IGameAction
{
    private readonly IPiece _piece;
    private readonly ITargetedModifier _modifier;
    private readonly Vector3Int _targetCell;
    private readonly Vector3Int _gridSize;
    private readonly ModifierHost _modifierHost;
    private readonly PieceEventBus _eventBus;

    public TargetedModifierAction(
        IPiece piece,
        ITargetedModifier modifier,
        Vector3Int targetCell,
        Vector3Int gridSize,
        ModifierHost modifierHost,
        PieceEventBus eventBus)
    {
        _piece = piece ?? throw new ArgumentNullException(nameof(piece));
        _modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _targetCell = targetCell;
        _gridSize = gridSize;
    }

    public IdOfPlayer Owner => _piece.Owner;
    public IPiece? FocusPiece => _piece;
    public ITargetedModifier Modifier => _modifier;
    public Vector3Int TargetCell => _targetCell;

    public bool CanExecute()
    {
        if (_piece.Health <= 0) return false;
        if (_modifier.CurrentCharges == 0) return false;

        var model = _modifierHost.Registry.GetByPieceId(_piece.PieceId);
        if (model is null) return false;

        var ctx = new ModifierActivationContext(model, _eventBus, _modifierHost, _modifierHost.Registry);
        if (!_modifier.CanActivate(ctx)) return false;

        var cells = _modifier.GetTargetCells(model, _gridSize);
        for (int i = 0; i < cells.Count; i++)
            if (cells[i] == _targetCell) return true;
        return false;
    }

    public void Execute()
    {
        var model = _modifierHost.Registry.GetByPieceId(_piece.PieceId)
            ?? throw new InvalidOperationException($"PieceModel for {_piece} not found.");

        var ctx = new ModifierActivationContext(model, _eventBus, _modifierHost, _modifierHost.Registry);

        if (!_modifier.TryConsumeCharge())
            throw new InvalidOperationException(
                $"TargetedModifierAction: failed to consume charge of '{_modifier.ModifierName}'.");

        _modifier.Activate(ctx, _targetCell);
    }
}
