#nullable enable

public class GameStarter : IGameStarter
{
    private readonly IHouse _house;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly IFlow _flow; 
    private readonly IGameView _gameView;
    private readonly Seats _seats;
    private readonly BoardConfig _boardConfig;
    private readonly Board _board;
    private bool _started = false;

    public GameStarter(IHouse house, IEventBus eventBus, ILogger logger, IFlow flow, IGameView gameView, Seats seats, BoardConfig boardConfig, Board board) 
    {
        _house = house;
        _eventBus = eventBus;
        _logger = logger;
        _flow = flow;
        _gameView = gameView;
        _seats = seats;
        _boardConfig = boardConfig;
        _board = board;
    }

    public void StartGame()
    {
        if (_started) return;
        _started = true;
        
        _house.AddRoom(_board);
        var decks = new Decks(_seats, _eventBus, _logger);
        _house.AddRoom(decks);

        if (_gameView is GameView view)
        {
            view.Initialize(_eventBus);
            var axes = (MultipleAxes)_boardConfig.Axes;
            var box = new AxisAlignedBox(axes, _boardConfig.FieldSize, 0);
            view.BuildGrid(box, axes);
        }

        var definition = new PieceDefinition("Pawn", new IndexOfPlayer(0, 0), 10);
        _flow.AddPiece<Board, Square>(definition, Square.Zero);
    }
}