#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public sealed class Registry : IRegistry
{
    private readonly Dictionary<BigInteger, IRoom> _indexToRoom = new();
    private readonly Dictionary<BigInteger, IPiece> _indexToPiece = new();
    private readonly List<IPiece> _allPieces = new();
    private readonly List<IDisposable> _subscriptions = new();
    private readonly IHouse _house;
    private bool _disposed;

    public Registry(IHouse house)
    {
        _house = house ?? throw new ArgumentNullException(nameof(house));
        house.RoomAdded += OnRoomAdded;
        foreach (var room in house.Rooms.Values) SubscribeToRoom(room);
    }

    public IReadOnlyCollection<IPiece> AllPieces => _allPieces;

    private void OnRoomAdded(IRoom room) => SubscribeToRoom(room);
    private void SubscribeToRoom(IRoom room)
    {
        room.PieceAdded += AddedHandler;
        room.PieceRemoved += RemovedHandler;
        _subscriptions.Add(new Subscription(() =>
        {
            room.PieceAdded -= AddedHandler;
            room.PieceRemoved -= RemovedHandler;
        }));

        void AddedHandler(IPiece piece, IRoomWithoutRemove _, bool __)
        {
            _indexToRoom[piece.IndexInHouse] = room;
            if (!_indexToPiece.ContainsKey(piece.IndexInHouse))
                _allPieces.Add(piece);
            _indexToPiece[piece.IndexInHouse] = piece;
        }
        void RemovedHandler(BigInteger index, IRoom __)
        {
            if (_indexToRoom.TryGetValue(index, out var currentRoom) && ReferenceEquals(currentRoom, __))
                _indexToRoom.Remove(index);
            if (_indexToPiece.Remove(index, out var piece))
                _allPieces.Remove(piece);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var sub in _subscriptions) sub.Dispose();
        _subscriptions.Clear();
        _indexToRoom.Clear();
        _indexToPiece.Clear();
        _allPieces.Clear();
        _house.RoomAdded -= OnRoomAdded;
    }
    public bool TryToGetRoom(BigInteger index, [NotNullWhen(true)] out IRoom? room)
    {
        return _indexToRoom.TryGetValue(index, out room);
    }
    public bool TryToGetPiece(BigInteger index, [NotNullWhen(true)] out IPiece? piece)
    {
        return _indexToPiece.TryGetValue(index, out piece);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;
        public void Dispose() => _unsubscribe?.Invoke();
    }
}