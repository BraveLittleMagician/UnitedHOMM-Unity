#nullable enable
using UnityEngine;

public sealed class TargetSelectionContext
{
    public IFlow Flow { get; init; } = null!;
    public ModifierHost ModifierHost { get; init; } = null!;
    public SelectionState SelectionState { get; init; } = null!;
    public ISelectionHub SelectionHub { get; init; } = null!;
    public MatchState MatchState { get; init; } = null!;
    public LoaderOfBoard BoardLoader { get; init; } = null!;
    public LayerPresenter LayerPresenter { get; init; } = null!;
    public ViewState ViewState { get; init; } = null!;
    public PendingModalManager PendingModalManager { get; init; } = null!;
    public Material MarkerMaterial { get; init; } = null!;
    public ILogger Logger { get; init; } = null!;
}