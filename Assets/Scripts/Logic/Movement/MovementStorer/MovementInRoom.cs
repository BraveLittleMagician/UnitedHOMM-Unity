#nullable enable

using System;
using System.Collections.Generic;

public abstract class MovementInRoom<TSequence, TRoom, TPosition> : Movement where TSequence : notnull, ISequence<TPosition, TSequence>, new() where TRoom : IRoomT<TPosition> where TPosition : struct
{
    private readonly IEventBus _eventBus;
    private readonly IPiece _owner;

    protected MovementInRoom(IEventBus eventBus, IPiece owner)
    {
        _eventBus = eventBus;
        _owner = owner;
        _eventBus.Subscribe<ModifiersUpdatedEvent>(OnModifiersUpdated);
        GeneralizedModifiers = new ModifiersOfMovement<TSequence, TPosition>(_eventBus, _owner);
    }

    protected TSequence ModifiedMovementSequence { get; set; } = new();
    protected TSequence ModifiedAttackSequence { get; set; } = new();
    protected abstract TSequence MovementSequence { get; }
    protected abstract TSequence AttackSequence { get; }

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
    protected abstract void CallUpdateModified();
    protected void UpdateModifedSequencies()
    {
        ModifiedMovementSequence = GetModifiedSequence(false);
        ModifiedAttackSequence = GetModifiedSequence(true);
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
    public override bool IsApplicableToRoomType(Type roomType)
    {
        if (typeof(TPosition) == typeof(Square)) return typeof(ISquarePositionRoom).IsAssignableFrom(roomType);
        if (typeof(TPosition) == typeof(int)) return typeof(IIntPositionRoom).IsAssignableFrom(roomType);
        return false;
    }
    public override void Dispose()
    {
        _eventBus.Unsubscribe<ModifiersUpdatedEvent>(OnModifiersUpdated);
        GC.SuppressFinalize(this);
    }
}