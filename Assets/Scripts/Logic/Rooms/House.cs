#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public sealed class House : IHouse
{
    private readonly Dictionary<string, IRoom> _rooms = new();
    private readonly Indexes _indexes = new();
    private readonly PieceFactory _pieceFactory;
    private readonly ILogger _logger;
    private bool _disposed;

    public House(Seats seats, IEventBus eventBus, ILogger logger, PieceFactory pieceFactory)
    {
        ActiveSeats = seats;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pieceFactory = pieceFactory ?? throw new ArgumentNullException(nameof(pieceFactory));
    }

    public Seats ActiveSeats { get; }
    public IReadOnlyDictionary<string, IRoom> Rooms => new ReadOnlyDictionary<string, IRoom>(_rooms);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var room in _rooms.Values) room.Dispose();
        _rooms.Clear();
        _indexes.Dispose();
    }
    public bool AddRoom<T>(T room) where T : IRoom
        => AddRoom(typeof(T).FullName ?? typeof(T).Name, room);
    public bool AddRoom<T>(string key, T room) where T : IRoom
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Room key cannot be empty", nameof(key));
        if (_rooms.ContainsKey(key))
        {
            _logger.LogWarning($"Room '{key}' already exists");
            return false;
        }
        _rooms[key] = room;
        RoomAdded?.Invoke(room);
        return true;
    }
    public bool TryToGetRoom<T>(out T room) where T : IRoom
    {
        foreach (var r in _rooms.Values)
            if (r is T typed) { room = typed; return true; }
        room = default!;
        return false;
    }
    public bool TryToGetRoom<T>(string key, out T room) where T : IRoom
    {
        if (_rooms.TryGetValue(key, out var r) && r is T typed)
        {
            room = typed;
            return true;
        }
        room = default!;
        return false;
    }
    public bool RemovePiece(BigInteger index)
    {
        _indexes.MakeFree(index);
        foreach (var iroom in _rooms.Values)
            if (iroom.Remove(index))
                return true;
        return false;
    }
    public IPiece CreatePiece(PieceDefinition definition)
    {
        var index = _indexes.GetNext();
        return _pieceFactory.Create(index, definition);
    }

    public event Action<IRoom>? RoomAdded;
}