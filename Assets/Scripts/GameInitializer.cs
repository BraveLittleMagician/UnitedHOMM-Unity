#nullable enable

using VContainer.Unity;

public class GameInitializer : IStartable
{
    private readonly IHouse _house;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Seats _seats;
    private readonly BoardConfig _boardConfig;
    private readonly IFlow _flow; 
    private readonly IGameView _gameView;

    public GameInitializer(IHouse house, IEventBus eventBus, ILogger logger, Seats seats, BoardConfig boardConfig, IFlow flow, IGameView gameView) 
    {
        _house = house;
        _eventBus = eventBus;
        _logger = logger;
        _seats = seats;
        _boardConfig = boardConfig;
        _flow = flow;
        _gameView = gameView;
    }

    public void Start()
    {
        var axes = _boardConfig.Is3D ? MultipleAxes.Three : MultipleAxes.Two;
        var box = new AxisAlignedBox(axes, _boardConfig.FieldSize + 2 * _boardConfig.FrameThickness, 0);
        var board = new Board(_seats, box, _eventBus, _logger);
        var decks = new Decks(_seats, _eventBus, _logger);

        _house.AddRoom(board);
        _house.AddRoom(decks);

        if (_gameView is GameView view)
            view.BuildGrid(_boardConfig, box);

        var definition = new PieceDefinition("Pawn", new IndexOfPlayer(0, 0), 10);
        _flow.AddPiece<Board, Square>(definition, new Square { X = 2 + _boardConfig.FrameThickness, Y = 2 + _boardConfig.FrameThickness });
    }
}