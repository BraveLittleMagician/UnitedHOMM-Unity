#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;

public interface IHouse : IDisposable
{
    Seats ActiveSeats { get; }
    IReadOnlyDictionary<string, IRoom> Rooms { get; }
    bool RemovePiece(BigInteger index);
    bool AddRoom<T>(T room) where T : IRoom;
    bool AddRoom<T>(string key, T room) where T : IRoom;
    bool TryToGetRoom<T>(out T room) where T : IRoom;
    bool TryToGetRoom<T>(string key, out T room) where T : IRoom;
    IPiece CreatePiece(PieceDefinition definition);

    event Action<IRoom>? RoomAdded;
}