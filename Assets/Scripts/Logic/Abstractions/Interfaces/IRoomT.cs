#nullable enable

using System;
using System.Numerics;

public interface IRoom : IRoomWithoutRemove
{
    public bool Remove(BigInteger index);
    public bool Remove<TPos>(TPos position, IndexOfPlayer player) where TPos : struct;

    public event Action<BigInteger, IRoom>? PieceRemoved;
}