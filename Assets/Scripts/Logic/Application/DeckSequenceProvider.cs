#nullable enable

using System;
using System.Collections.Generic;

public sealed class DeckSequenceProvider : IDisposable
{
    private readonly IEventBus _eventBus;
    private readonly Dictionary<IndexOfPlayer, SizeAwareInt> _cache = new();
    private bool _disposed;

    public DeckSequenceProvider(IEventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public SizeAwareInt GetFor(IndexOfPlayer player)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DeckSequenceProvider));

        if (!_cache.TryGetValue(player, out var sequence))
        {
            sequence = new SizeAwareInt(_eventBus, player);
            _cache[player] = sequence;
        }
        return sequence;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var seq in _cache.Values)
            seq.Dispose();
        _cache.Clear();
    }
}