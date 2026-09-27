#nullable enable

using System;

public sealed class MovementCombinedFactory<TSequence, TRoom, TPosition> : IMovementFactory where TSequence : notnull, ISequence<TPosition, TSequence> where TRoom : IRoomT<TPosition> where TPosition : struct
{
    private readonly TSequence _sequence;

    public MovementCombinedFactory(TSequence sequence)
    {
        _sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
    }

    public IMovement CreateFor(IEventBus eventBus, IPiece owner) => new MovementCombined<TSequence, TRoom, TPosition>(_sequence, eventBus, owner);
}