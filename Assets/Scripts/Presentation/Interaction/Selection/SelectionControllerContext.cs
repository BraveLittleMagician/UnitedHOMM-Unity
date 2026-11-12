#nullable enable

public sealed class SelectionControllerContext
{
    public IFlow Flow { get; init; } = null!;
    public IRegistry Registry { get; init; } = null!;
    public LoaderOfBoard BoardLoader { get; init; } = null!;
    public ViewState ViewState { get; init; } = null!;
    public LayerPresenter LayerPresenter { get; init; } = null!;
    public SelectionState SelectionState { get; init; } = null!;
    public HoverState HoverState { get; init; } = null!;
    public MatchState MatchState { get; init; } = null!;
    public TurnManager TurnManager { get; init; } = null!;
    public PendingModalManager PendingModalManager { get; init; } = null!;
    public ISelectionHub SelectionHub { get; init; } = null!;
    public IMarkerCoordinator MarkerCoordinator { get; init; } = null!;
    public ILogger Logger { get; init; } = null!;
}