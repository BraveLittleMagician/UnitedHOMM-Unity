#nullable enable

using System;

public sealed class RoomInitializer : IRoomInitializer
{
    private readonly IHouse _house;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private readonly Seats _seats;
    private readonly Board _board;

    public RoomInitializer(IHouse house, IEventBus eventBus, ILogger logger, Seats seats, Board board)
    {
        _house = house ?? throw new ArgumentNullException(nameof(house));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _board = board ?? throw new ArgumentNullException(nameof(board));
        _seats = seats;
    }

    public void InitializeRooms()
    {
        if (!_house.AddRoom(_board))
            _logger.LogWarning($"Комната {_board.Name} уже была в доме");

        var decks = new Decks(_seats, _eventBus, _logger);

        if (!_house.AddRoom(decks))
            _logger.LogWarning($"Комната {decks.Name} уже была в доме");

        _logger.Log($"Комнаты инициализированы: {_board.Name}, {decks.Name}");
    }
}