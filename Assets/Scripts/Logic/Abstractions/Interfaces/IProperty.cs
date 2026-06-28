#nullable enable

using System;

public interface IProperty : IDisposable
{
    public int Value { get; }
}