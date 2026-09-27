#nullable enable

using System;

public sealed class MovementSeparatedFactory<TSequence, TRoom, TPosition> : IMovementFactory
    where TSequence : notnull, ISequence<TPosition, TSequence>
    where TRoom : IRoomT<TPosition>
    where TPosition : struct
{
    private readonly TSequence _moveSequence;
    private readonly TSequence _attackSequence;

    public MovementSeparatedFactory(TSequence moveSequence, TSequence attackSequence)
    {
        _moveSequence = moveSequence ?? throw new ArgumentNullException(nameof(moveSequence));
        _attackSequence = attackSequence ?? throw new ArgumentNullException(nameof(attackSequence));
    }

    public IMovement CreateFor(IEventBus eventBus, IPiece owner) =>
        new MovementSepareted<TSequence, TRoom, TPosition>(_moveSequence, _attackSequence, eventBus, owner);
}