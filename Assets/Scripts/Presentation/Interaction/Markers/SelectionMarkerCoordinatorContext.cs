#nullable enable

using System.Collections.Generic;
using UnityEngine;

public sealed class SelectionMarkerCoordinatorContext
{
    public ViewState ViewState { get; init; } = null!;
    public LayerPresenter LayerPresenter { get; init; } = null!;
    public LoaderOfBoard BoardLoader { get; init; } = null!;
    public SelectionState SelectionState { get; init; } = null!;
    public IRegistry Registry { get; init; } = null!;
    public PendingModalManager PendingModalManager { get; init; } = null!;
    public MatchState MatchState { get; init; } = null!;
    public Material MarkerMaterial { get; init; } = null!;
    public IReadOnlyList<ICellActionProvider> CellActionProviders { get; init; } = null!;
}