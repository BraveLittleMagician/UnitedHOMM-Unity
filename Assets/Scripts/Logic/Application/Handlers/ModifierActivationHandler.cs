#nullable enable
using System;
using System.Collections.Generic;

public sealed class ModifierActivationHandler
{
    private readonly IFlow _flow;
    private readonly PendingModalManager _modalManager;
    private readonly MatchState _matchState;
    private readonly ModifierHost _modifierHost;
    private readonly IRegistry _registry;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    public event Action? StateChanged;
    public event Action<IPiece, IActiveModifier>? ModifierActivated;

    public ModifierActivationHandler(ModifierActivationHandlerContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _flow = context.Flow;
        _modalManager = context.PendingModalManager;
        _matchState = context.MatchState;
        _modifierHost = context.ModifierHost;
        _registry = context.Registry;
        _eventBus = context.EventBus;
        _logger = context.Logger;
    }

    public IReadOnlyList<IActiveModifier> GetSelectedActiveModifiers(IPiece piece)
    {
        if (piece == null || piece.IsDead) return Array.Empty<IActiveModifier>();
        if (_matchState.IsOver) return Array.Empty<IActiveModifier>();
        if (piece.HasPromotedThisTurn) return Array.Empty<IActiveModifier>();

        var modifiers = _modifierHost.GetModifiers(piece.IndexInHouse);
        var result = new List<IActiveModifier>();
        for (int i = 0; i < modifiers.Count; i++)
            if (modifiers[i] is IActiveModifier active)
                result.Add(active);
        return result;
    }

    public bool CanActivateModifier(IPiece piece, IActiveModifier modifier)
    {
        if (piece == null || modifier == null) return false;
        if (piece.IsDead) return false;
        if (_matchState.IsOver) return false;
        if (_modalManager.HasPending) return false;
        if (piece.HasPromotedThisTurn) return false;
        if (modifier.CurrentCharges == 0) return false;

        var ctx = new ModifierActivationContext(piece, _eventBus, _modifierHost, _registry);
        return modifier.CanActivate(ctx);
    }

    public void HandleModifierActivate(IPiece piece, IActiveModifier modifier)
    {
        if (_matchState.IsOver) return;
        if (_modalManager.HasPending) return;
        if (!CanActivateModifier(piece, modifier)) return;

        var result = _flow.ActivateModifier(piece, modifier);
        if (!result.IsSuccess)
        {
            _logger.LogWarning($"[MODIFIER] Activate failed: {result.Error}");
            return;
        }
        ModifierActivated?.Invoke(piece, modifier);
        StateChanged?.Invoke();
    }

    public void HandleTargetedModifierActivate(IPiece piece, ITargetedModifier modifier, Square target)
    {
        if (_matchState.IsOver) return;
        if (_modalManager.HasPending) return;
        if (!CanActivateModifier(piece, modifier)) return;

        var result = _flow.ActivateTargetedModifier(piece, modifier, target);
        if (!result.IsSuccess)
        {
            _logger.LogWarning($"[MODIFIER] Targeted activate failed: {result.Error}");
            return;
        }
        ModifierActivated?.Invoke(piece, modifier);
        StateChanged?.Invoke();
    }
}