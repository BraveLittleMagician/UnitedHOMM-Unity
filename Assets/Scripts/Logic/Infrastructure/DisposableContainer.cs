#nullable enable

using System;
using System.Collections.Generic;

public class DisposableContainer : IDisposable
{
    private readonly List<IDisposable> _items = new();
    private bool _disposed;

    public void Add(IDisposable item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (_disposed) throw new ObjectDisposedException(nameof(DisposableContainer));
        _items.Add(item);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        for (int i = _items.Count - 1; i >= 0; i--) _items[i]?.Dispose();
        _items.Clear();
        GC.SuppressFinalize(this);
    }
}