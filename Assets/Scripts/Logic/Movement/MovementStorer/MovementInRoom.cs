#nullable enable

using System;
using System.Collections.Generic;

public abstract class MovementInRoom<TSequence, TRoom, TPosition> : Movement where TSequence : notnull, ISequence<TPosition, TSequence> where TRoom : IRoomT<TPosition> where TPosition : struct
{
    private readonly IEventBus _eventBus;
    private readonly IPiece _owner;

    protected MovementInRoom(TSequence movementSequence, TSequence attackSequence, IEventBus eventBus, IPiece owner)
    {
        MovementSequence = movementSequence ?? throw new ArgumentNullException(nameof(movementSequence));
        AttackSequence = attackSequence ?? throw new ArgumentNullException(nameof(attackSequence));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus)); ;
        _owner = owner ?? throw new ArgumentNullException(nameof(owner)); ;
        _eventBus.Subscribe<ModifiersUpdatedEvent>(OnModifiersUpdated);
        GeneralizedModifiers = new ModifiersOfMovement<TSequence, TPosition>(_eventBus, _owner);

        ModifiedMovementSequence = GetModifiedSequence(isAttack: false);
        ModifiedAttackSequence = GetModifiedSequence(isAttack: true);
    }

    protected TSequence MovementSequence { get; }
    protected TSequence AttackSequence { get; }

    protected TSequence ModifiedMovementSequence { get; private set; }
    protected TSequence ModifiedAttackSequence { get; private set; }

    protected ModifiersOfMovement<TSequence, TPosition> GeneralizedModifiers { get; } 

    public override Modifiers Modifiers => GeneralizedModifiers;

    private void OnModifiersUpdated(ModifiersUpdatedEvent e)
    {
        if (e.Piece == _owner && e.SequenceType == typeof(TSequence))
        {
            if (e.IsAttack)
                ModifiedAttackSequence = GetModifiedSequence(true);
            else
                ModifiedMovementSequence = GetModifiedSequence(false);
        }
    }
    private TSequence GetModifiedSequence(bool isAttack)
    {
        var sequence = isAttack ? AttackSequence : MovementSequence;
        foreach (var modifier in isAttack ? GeneralizedModifiers.Attacks : GeneralizedModifiers.Movements)
            sequence = modifier.Apply(sequence);
        return sequence;
    }

    public abstract List<TPosition> GetStayables(TPosition start, bool isAttack);
    public override List<TPos> GetStayables<TPos>(TPos tpos, bool isAttack)
    {
        if (tpos is TPosition start)
        {
            var result = GetStayables(start, isAttack);
            if (result is List<TPos> posResult) return posResult;
        }
        return new List<TPos>();
    }
    public abstract Dictionary<TPosition, Stayable> GetPositions(TPosition start, bool isAttack);
    public override Dictionary<TPos, Stayable> GetPositions<TPos>(TPos tpos, bool isAttack) where TPos : struct
    {
        if (tpos is TPosition start)
        {
            var result = GetPositions(start, isAttack);
            if (result is Dictionary<TPos, Stayable> posResult) return posResult;
        }
        return new Dictionary<TPos, Stayable>();
    }
    public override bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, bool isAttack) where TPos : struct
    {
        if (path is IPath<TPosition> typedPath)
            return CanMove(piece, typedPath, room, isAttack);
        return false;
    }
    public bool CanMove(IPath<TPosition> path, bool isAttack)
    {
        var sequence = isAttack ? ModifiedAttackSequence.GetEnumerator(path.Positions[0]) : ModifiedMovementSequence.GetEnumerator(path.Positions[0]);
        if (!sequence.CanMoveTo(path.Positions[^1])) return false;
        if (!sequence.CanMoveTo(path)) return false;
        return true;
    }
    public override bool IsApplicableToRoom(IReadOnlyRoom room)
    {
        if (typeof(TPosition) == typeof(int))
            return room is IIntPositionRoom;
        if (typeof(ISquarePosition).IsAssignableFrom(typeof(TPosition)))
            return room is ISquarePositionRoom;
        return false;
    }
    public override void Dispose()
    {
        _eventBus.Unsubscribe<ModifiersUpdatedEvent>(OnModifiersUpdated);
        GeneralizedModifiers.Dispose();
    }
}