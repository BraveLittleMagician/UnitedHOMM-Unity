#nullable enable

using System.Collections.Generic;

public class MovementCombined<TSequence, TRoom, TPosition> : MovementInRoom<TSequence, TRoom, TPosition> where TSequence : notnull, ISequence<TPosition, TSequence> where TRoom : IRoomT<TPosition> where TPosition : struct
{
    public MovementCombined(TSequence moveAttackSequence, IEventBus eventBus, IPiece owner) : base(moveAttackSequence, moveAttackSequence, eventBus, owner) { }

    public override List<TPosition> GetStayables(TPosition start, bool isAttack) => ModifiedMovementSequence.GetEnumerator(start).PositionsStayables;
    public override Dictionary<TPosition, Stayable> GetPositions(TPosition start, bool isAttack) => ModifiedMovementSequence.GetEnumerator(start).PositionsAll;
}