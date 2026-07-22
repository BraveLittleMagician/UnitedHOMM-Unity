#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public interface IRegistry : IDisposable
{
    bool TryToGetRoom([NotNullWhen(true)] BigInteger index, out IRoom? room);
}