#nullable enable
using System.Collections.Generic;

public sealed class CellActionHandlerContext
{
    public IFlow Flow { get; init; } = null!;
    public PendingModalManager PendingModalManager { get; init; } = null!;
    public MatchState MatchState { get; init; } = null!;
    public IReadOnlyList<ICellActionProvider> CellActionProviders { get; init; } = null!;
    public ILogger Logger { get; init; } = null!;
}