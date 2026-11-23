#nullable enable
public sealed class MovementCombined<TSequence> : MovementInRoom<TSequence> where TSequence : notnull, ISequence<Square, TSequence>
{
    public MovementCombined(TSequence sequence, IEventBus eventBus, ModifierHost modifierHost, IPiece owner) : base(sequence, sequence, eventBus, modifierHost, owner) { }

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
        var transforms = GetShapeTransforms(piece);
        bool hasAttackModifier = false;
        for (int i = 0; i < transforms.Count; i++)
            if ((transforms[i].Target & ShapeTarget.Attack) != 0) { hasAttackModifier = true; break; }

        if (!hasAttackModifier)
            return WithAttackSemantics(CreateMoveEnumeratorFor(piece, fromPosition));

        var moveModified = ApplyMovementModifiers(MovementSequence, piece);
        var attackModified = ApplyAttackModifiers(AttackSequence, piece);
        var startLocal = FacingRotation.Unrotate(fromPosition, piece.Facing);
        var combined = new CombinedEnumerator(
            moveModified.GetEnumerator(startLocal),
            attackModified.GetEnumerator(startLocal));
        return WrapWithFacing(combined, piece, fromPosition);
    }
}