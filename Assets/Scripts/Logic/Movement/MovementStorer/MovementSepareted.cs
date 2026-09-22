#nullable enable

using System.Collections.Generic;

public class MovementSepareted<TSequence, TRoom, TPosition> : MovementInRoom<TSequence, TRoom, TPosition> where TSequence : notnull, ISequence<TPosition, TSequence> where TRoom : IRoomT<TPosition> where TPosition : struct
{
    private readonly TSequence _moveSequence;
    private readonly TSequence _attackSequence;

    public MovementSepareted(TSequence moveSequence, TSequence attackSequence, IEventBus eventBus, IPiece owner) : base(eventBus, owner)
    {
        _moveSequence = moveSequence;
        _attackSequence = attackSequence;
        CallUpdateModified();
    }

    protected override TSequence MovementSequence => _moveSequence;
    protected override TSequence AttackSequence => _attackSequence;

    protected override void CallUpdateModified() => UpdateModifedSequencies();
    public override List<TPosition> GetStayables(TPosition start, bool isAttack)
    {
        if (isAttack) return ModifiedAttackSequence.GetEnumerator(start).PositionsStayables;
        else return ModifiedMovementSequence.GetEnumerator(start).PositionsStayables;
    }
    public override Dictionary<TPosition, Stayable> GetPositions(TPosition start, bool isAttack)
    {
        if (isAttack) return ModifiedAttackSequence.GetEnumerator(start).PositionsAll;
        else return ModifiedMovementSequence.GetEnumerator(start).PositionsAll;
    }
}