#nullable enable

using System;
using System.Collections.Generic;

public sealed class ModifiersOfMovement<TSequence, TPosition> : IDisposable where TSequence : notnull, ISequence<TPosition, TSequence> where TPosition : struct, ISquare<TPosition>
{
    private readonly IEventBus _eventBus;
    private readonly IPiece _owner;
    private readonly Dictionary<Type, ModifierOfMovement<TSequence, TPosition>> _modifiers = new();
    private bool _disposed;

    public ModifiersOfMovement(IEventBus eventBus, IPiece owner)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public IEnumerable<ModifierOfMovement<TSequence, TPosition>> All => _modifiers.Values;

    public bool Add<TModif>(TModif modifier)
        where TModif : ModifierOfMovement<TSequence, TPosition>
    {
        if (modifier == null) throw new ArgumentNullException(nameof(modifier));
        if (_disposed) throw new ObjectDisposedException(nameof(ModifiersOfMovement<TSequence, TPosition>));

        if (!_modifiers.TryAdd(typeof(TModif), modifier))
            return false;

        _eventBus.Publish(new ModifiersUpdatedEvent(_owner, typeof(TSequence)));
        return true;
    }

    public bool Remove<TModif>()
        where TModif : ModifierOfMovement<TSequence, TPosition>
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ModifiersOfMovement<TSequence, TPosition>));

        if (!_modifiers.Remove(typeof(TModif)))
            return false;

        _eventBus.Publish(new ModifiersUpdatedEvent(_owner, typeof(TSequence)));
        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _modifiers.Clear();
    }
}