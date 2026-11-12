#nullable enable
using System;
using System.Collections.Generic;

public sealed record DomainContext(
    Seats Seats,
    Board Board,
    IHouse House,
    IRegistry Registry,
    IFlow Flow,
    TurnManager TurnManager,
    MatchState MatchState,
    ModifierHost ModifierHost,
    ReactiveActionRegistry ReactiveRegistry,
    IAbilityService AbilityService,
    DeckSequenceProvider DeckSequences,
    PendingModalManager PendingModalManager,
    CellActionHandler CellActionHandler,
    ModifierActivationHandler ModifierActivationHandler,
    PromotionChoiceHandler PromotionChoiceHandler,
    IReadOnlyList<ICellActionProvider> CellActionProviders,
    IReadOnlyList<StartingPlacement> Placements,
    PiecePrefabRegistry PiecePrefabs)
{
    public void Dispose()
    {
        SafeDispose(ReactiveRegistry);
        SafeDispose(DeckSequences);
        SafeDispose(AbilityService);
        SafeDispose(ModifierHost);
        SafeDispose(House);
    }

    private static void SafeDispose(IDisposable? d)
    {
        if (d == null) return;
        try { d.Dispose(); }
        catch (Exception ex) { UnityEngine.Debug.LogError($"[DomainContext.Dispose] {ex}"); }
    }
}