#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

public interface IRegistry : IDisposable
{
    bool TryToGetPosition<TPos>(BigInteger index, out TPos position) where TPos : struct;
    bool TryToGetRoom([NotNullWhen(true)] BigInteger index, out IRoom? room);
}