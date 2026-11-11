#nullable enable
public sealed class PromotionChoiceHandlerContext
{
    public PendingModalManager PendingModalManager { get; init; } = null!;
    public MatchState MatchState { get; init; } = null!;
    public PromotionService PromotionService { get; init; } = null!;
    public ILogger Logger { get; init; } = null!;
}
