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
        var axes = (MultipleAxes)_boardConfig.Axes;
        var box = new AxisAlignedBox(axes, _boardConfig.FieldSize, 0);
        var board = new Board(_seats, box, _eventBus, _logger);
        var decks = new Decks(_seats, _eventBus, _logger);

        _house.AddRoom(board);
        _house.AddRoom(decks);

        if (_gameView is GameView view)
            view.BuildGrid(box, axes);

        var definition = new PieceDefinition("Pawn", new IndexOfPlayer(0, 0), 10);
        _flow.AddPiece<Board, Square>(definition, Square.Zero);
    }
}