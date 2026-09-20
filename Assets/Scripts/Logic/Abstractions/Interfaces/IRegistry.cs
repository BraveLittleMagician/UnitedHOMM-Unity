#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public interface IRegistry : IDisposable
{
    bool TryToGetRoom(BigInteger index, [NotNullWhen(true)] out IRoom? room);
}