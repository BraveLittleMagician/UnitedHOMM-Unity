#nullable enable

using System.Collections.Generic;

public class MovementCombined<TSequence, TRoom, TPosition> : MovementInRoom<TSequence, TRoom, TPosition> where TSequence : notnull, ISequence<TPosition, TSequence>, new() where TRoom : IRoomT<TPosition> where TPosition : struct
{
    private readonly TSequence _moveAttackSequence;

    public MovementCombined(TSequence moveSequence, IEventBus eventBus, IPiece owner) : base(eventBus, owner)
    {
        _moveAttackSequence = moveSequence;
        CallUpdateModified();
    }

    protected override TSequence MovementSequence => _moveAttackSequence;
    protected override TSequence AttackSequence => _moveAttackSequence;

    protected override void CallUpdateModified()
    {
        UpdateModifedSequencies();
    }
    public override List<TPosition> GetStayables(TPosition start, bool isAttack) => ModifiedMovementSequence.GetEnumerator(start).PositionsStayables;
    public override Dictionary<TPosition, Stayable> GetPositions(TPosition start, bool isAttack) => ModifiedMovementSequence.GetEnumerator(start).PositionsAll;
}