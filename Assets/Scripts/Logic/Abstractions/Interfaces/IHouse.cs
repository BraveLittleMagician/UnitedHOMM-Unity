#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;

public interface IHouse : IDisposable
{
    Seats ActiveSeats { get; }
    IReadOnlyDictionary<Type, IRoom> Rooms { get; }

    void RemovePiece(BigInteger index);
    bool AddRoom<T>(T room) where T : IRoom;
    bool TryToGetRoom<T>(out T room) where T : IRoom;
    IPiece CreatePiece(PieceDefinition definition);

    event Action<IRoom>? RoomAdded;
}