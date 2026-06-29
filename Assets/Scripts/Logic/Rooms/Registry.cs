#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public sealed class Registry : IRegistry
{
    private readonly Dictionary<BigInteger, IRoom> _indexToRoom = new();
    private readonly List<IDisposable> _subscriptions = new();
    private bool _disposed;

    public Registry(IHouse house, IEventBus eventBus)
    {
        foreach (var room in house.Rooms.Values)
        {
            void AddedHandler(IPiece piece, IRoomWithoutRemove roomWithoutRemove, bool fromAnotherRoom)
            {
                if (room is IRoom r)
                    _indexToRoom[piece.IndexInHouse] = r;
            }

            void RemovedHandler(BigInteger index, IRoom room)
            {
                _indexToRoom.Remove(index);
            }

            room.PieceAdded += AddedHandler;
            room.PieceRemoved += RemovedHandler;

            _subscriptions.Add(new Subscription(() =>
            {
                room.PieceAdded -= AddedHandler;
                room.PieceRemoved -= RemovedHandler;
            }));
        }
    }

    public bool TryGetRoomByIndex([NotNullWhen(true)] BigInteger index, out IRoom? room)
    {
        return _indexToRoom.TryGetValue(index, out room);
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var sub in _subscriptions)
            sub.Dispose();
        _subscriptions.Clear();
        _indexToRoom.Clear();
    }

    private class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;
        public void Dispose() => _unsubscribe?.Invoke();
    }
}