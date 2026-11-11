#nullable enable

public sealed class MovementFactory<TSequence> : IMovementFactory where TSequence : notnull, ISequence<Square, TSequence>
{
    private readonly TSequence _moveSequence;
    private readonly TSequence? _attackSequence;
    private readonly bool _hasAttackSequence;

    public MovementFactory(TSequence moveSequence)
        : this(moveSequence, default!, hasAttackSequence: false) { }

    public MovementFactory(TSequence moveSequence, TSequence attackSequence)
        : this(moveSequence, attackSequence, hasAttackSequence: true) { }

    private MovementFactory(TSequence moveSequence, TSequence? attackSequence, bool hasAttackSequence)
    {
        _moveSequence = moveSequence;
        _attackSequence = attackSequence;
        _hasAttackSequence = hasAttackSequence;
    }

    public IMovement CreateFor(IEventBus eventBus, ModifierHost modifierHost, IPiece owner)
        => new Movement<TSequence>(_moveSequence, _attackSequence, _hasAttackSequence, eventBus, modifierHost, owner);
}