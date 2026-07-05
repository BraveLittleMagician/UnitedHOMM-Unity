#nullable enable

using VContainer.Unity;

public class GameInitializer : IStartable
{
    private readonly IHouse _house;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Seats _seats;
    private readonly BoardConfig _boardConfig;
    private readonly IFlow _controller;

    public GameInitializer(IHouse house, IEventBus eventBus, ILogger logger, Seats seats, BoardConfig boardConfig, IFlow controller) 
    {
        _house = house;
        _eventBus = eventBus;
        _logger = logger;
        _seats = seats;
        _boardConfig = boardConfig;
        _controller = controller;
    }

    public void Start()
    {
        var axes = _boardConfig.Is3D ? MultipleAxes.Three : MultipleAxes.Two;
        var box = new AxisAlignedBox(axes, _boardConfig.FieldSize, 0);
        var board = new Board(_seats, box, _eventBus, _logger);
        var decks = new Decks(_seats, _eventBus, _logger);

        _house.AddRoom(board);
        _house.AddRoom(decks);

        var definition = new PieceDefinition("Pawn", new IndexOfPlayer(0, 0), 10);
        _controller.AddPiece<Board, Square>(definition, new Square { X = 2, Y = 2 });
    }
}