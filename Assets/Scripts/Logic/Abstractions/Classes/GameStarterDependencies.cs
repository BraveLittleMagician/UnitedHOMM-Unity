#nullable enable

public record GameStarterDependencies(
    IControllerOfCamera Camera,
    IHouse House,
    IEventBus EventBus,
    ILogger Logger,
    IFlow Flow,
    IGameView GameView,
    Seats Seats,
    BoardConfig BoardConfig,
    Board Board
);