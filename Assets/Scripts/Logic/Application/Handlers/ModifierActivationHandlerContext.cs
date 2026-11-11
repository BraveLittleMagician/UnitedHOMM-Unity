#nullable enable
public sealed class ModifierActivationHandlerContext
{
    public IFlow Flow { get; init; } = null!;
    public PendingModalManager PendingModalManager { get; init; } = null!;
    public MatchState MatchState { get; init; } = null!;
    public ModifierHost ModifierHost { get; init; } = null!;
    public IRegistry Registry { get; init; } = null!;
    public IEventBus EventBus { get; init; } = null!;
    public ILogger Logger { get; init; } = null!;
}