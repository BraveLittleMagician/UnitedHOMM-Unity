#nullable enable

using System;

public sealed class ModifierAction : IGameAction
{
    private readonly IPiece _piece;
    private readonly IActiveModifier _modifier;
    private readonly ModifierHost _modifierHost;
    private readonly PieceEventBus _eventBus;

    public ModifierAction(
        IPiece piece,
        IActiveModifier modifier,
        ModifierHost modifierHost,
        PieceEventBus eventBus)
    {
        _piece = piece ?? throw new ArgumentNullException(nameof(piece));
        _modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));
        _modifierHost = modifierHost ?? throw new ArgumentNullException(nameof(modifierHost));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public IdOfPlayer Owner => _piece.Owner;
    public IPiece? FocusPiece => _piece;
    public IActiveModifier Modifier => _modifier;

    public bool CanExecute()
    {
        if (_piece.Health <= 0) return false;
        if (_modifier.CurrentCharges == 0) return false;

        var model = _modifierHost.Registry.GetByPieceId(_piece.PieceId);
        if (model is null) return false;

        var ctx = new ModifierActivationContext(model, _eventBus, _modifierHost, _modifierHost.Registry);
        return _modifier.CanActivate(ctx);
    }

    public void Execute()
    {
        var model = _modifierHost.Registry.GetByPieceId(_piece.PieceId)
            ?? throw new InvalidOperationException($"PieceModel for {_piece} not found.");

        var ctx = new ModifierActivationContext(model, _eventBus, _modifierHost, _modifierHost.Registry);

        if (!_modifier.TryConsumeCharge())
            throw new InvalidOperationException(
                $"ModifierAction: failed to consume charge of '{_modifier.ModifierName}'.");

        _modifier.Activate(ctx);
    }
}