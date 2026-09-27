#nullable enable

using System;
using System.Collections.Generic;

public sealed class DisposableContainer : IDisposable
{
    private readonly List<IDisposable> _items = new();
    private readonly ILogger? _logger;
    private bool _disposed;

    public DisposableContainer(ILogger? logger = null)
    {
        _logger = logger;
    }

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

        List<Exception>? exceptions = null;
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            try
            {
                _items[i].Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Ошибка при Dispose {_items[i].GetType().Name}: {ex}");
                (exceptions ??= new()).Add(ex);
            }
        }
        _items.Clear();

        if (exceptions != null && exceptions.Count == 1) throw exceptions[0];
        if (exceptions != null) throw new AggregateException(exceptions);
    }
}