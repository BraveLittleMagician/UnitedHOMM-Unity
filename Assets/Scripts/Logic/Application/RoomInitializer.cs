#nullable enable

public sealed class RoomInitializer : IRoomInitializer
{
    private readonly IHouse _house;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Seats _seats;
    private readonly BoardConfig _boardConfig;
    private readonly Board _board;

    public RoomInitializer(IHouse house, IEventBus eventBus, ILogger logger, Seats seats, BoardConfig boardConfig, Board board)
    {
        _house = house;
        _eventBus = eventBus;
        _logger = logger;
        _seats = seats;
        _boardConfig = boardConfig;
        _board = board;
    }

    public void InitializeRooms()
    {
        _house.AddRoom(_board);
        var decks = new Decks(_seats, _eventBus, _logger);
        _house.AddRoom(decks);
    }
}