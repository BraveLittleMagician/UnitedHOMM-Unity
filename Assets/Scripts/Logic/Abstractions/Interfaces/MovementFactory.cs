#nullable enable

using System;

public sealed class MovementFactory<TSequence, TPosition> : IMovementFactory where TSequence : notnull, ISequence<TPosition, TSequence> where TPosition : struct
{
    private readonly TSequence _sequence;

    public MovementFactory(TSequence sequence)
        => _sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));

    public IMovement CreateFor(IEventBus eventBus, IPiece owner)
        => new Movement<TSequence, TPosition>(_sequence, eventBus, owner);
}