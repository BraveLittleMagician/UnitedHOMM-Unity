#nullable enable


using System;
using System.Collections.Generic;
using System.Numerics;

public sealed class Indexes : IDisposable
{
    private readonly SortedSet<BigInteger> _freeIndices = new();
    private BigInteger _lastIndex = -1;

    public BigInteger GetNext()
    {
        if (_freeIndices.Count == 0)
        {
            _lastIndex++;
            return _lastIndex;
        }
        var first = _freeIndices.Min;
        _freeIndices.Remove(first);
        return first;
    }

    public void MakeFree(BigInteger index) => _freeIndices.Add(index);

    public void Dispose()
    {
        _freeIndices.Clear();
        _lastIndex = -1;
    }
}