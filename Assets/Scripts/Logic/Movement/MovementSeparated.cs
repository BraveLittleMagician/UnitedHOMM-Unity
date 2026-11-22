#nullable enable
public sealed class MovementSeparated<TSequence> : MovementInRoom<TSequence> where TSequence : notnull, ISequence<Square, TSequence>
{
    public MovementSeparated(
        TSequence moveSequence,
        TSequence attackSequence,
        IEventBus eventBus,
        ModifierHost modifierHost,
        IPiece owner)
        : base(moveSequence, attackSequence, eventBus, modifierHost, owner)
    {
    }

    public override ISequenceEnumerator<Square> CreateMoveEnumeratorFor(IPiece piece, Square fromPosition)
    {
        var modified = ApplyMovementModifiers(MovementSequence, piece);
        var startLocal = FacingRotation.Unrotate(fromPosition, piece.Facing);
        return WrapWithFacing(modified.GetEnumerator(startLocal), piece, fromPosition);
    }

    public override ISequenceEnumerator<Square> CreateAttackEnumeratorFor(IPiece piece, Square fromPosition)
    {
        var modified = ApplyAttackModifiers(AttackSequence, piece);
        var startLocal = FacingRotation.Unrotate(fromPosition, piece.Facing);
        return WithAttackSemantics(WrapWithFacing(modified.GetEnumerator(startLocal), piece, fromPosition));
    }

    public override ISequenceEnumerator<Square> CreateCombinedEnumeratorFor(IPiece piece, Square fromPosition)
    {
        var moveModified = ApplyMovementModifiers(MovementSequence, piece);
        var attackModified = ApplyAttackModifiers(AttackSequence, piece);
        var startLocal = FacingRotation.Unrotate(fromPosition, piece.Facing);
        var combined = new CombinedEnumerator(
            moveModified.GetEnumerator(startLocal),
            attackModified.GetEnumerator(startLocal));
        return WrapWithFacing(combined, piece, fromPosition);
    }
}
