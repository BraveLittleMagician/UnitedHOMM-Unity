#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public interface IRegistry : IDisposable
{
    bool TryToGetRoom(BigInteger index, [NotNullWhen(true)] out IRoom? room);
    bool TryToGetPiece(BigInteger index, [NotNullWhen(true)] out IPiece? piece);
    IReadOnlyCollection<IPiece> AllPieces { get; }
}