#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;

public sealed class House : IHouse
{
    private readonly Dictionary<Type, IRoom> _rooms = new();
    private readonly Indexes _indexes = new();
    private readonly PieceFactory _pieceFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private bool _disposed;

    public House(Seats seats, IEventBus eventBus, ILogger logger)
    {
        ActiveSeats = seats;
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pieceFactory = new PieceFactory(eventBus, logger);

        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
    }

    public Seats ActiveSeats { get; }
    public IReadOnlyDictionary<Type, IRoom> Rooms => _rooms;

    private void OnPieceDied(PieceDiedEvent e)
    {
        RemovePiece(e.Piece.IndexInHouse);
        _logger.Log($"Индекс {e.Piece.IndexInHouse} фигуры {e.Piece} освобождён");
    }

    public bool AddRoom<T>(T room) where T : IRoom
    {
        var type = typeof(T);
        if (_rooms.ContainsKey(type))
            return false;

        _rooms[type] = room;
        RoomAdded?.Invoke(room);
        return true;
    }
    public bool TryToGetRoom<T>(out T room) where T : IRoom
    {
        if (_rooms.TryGetValue(typeof(T), out var r) && r is T typed)
        {
            room = typed;
            return true;
        }
        room = default!;
        return false;
    }
    public void RemovePiece(BigInteger index)
    {
        _indexes.MakeFree(index);
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);

        foreach (var room in _rooms.Values) room.Dispose();
        _rooms.Clear();
        _indexes.Dispose();
    }
    public IPiece CreatePiece(PieceDefinition definition)
    {
        var index = _indexes.GetNext();
        return _pieceFactory.Create(index, definition);
    }
    
    public event Action<IRoom>? RoomAdded;
}