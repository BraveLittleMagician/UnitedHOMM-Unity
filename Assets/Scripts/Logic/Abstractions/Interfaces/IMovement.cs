#nullable enable

using System;
using System.Collections.Generic;

public interface IMovement 
{
    bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, bool isAttack) where TPos : struct;
    bool IsApplicableToRoomType(Type roomType);
    List<TPos> GetStayables<TPos>(TPos tpos, bool isAttack) where TPos : struct;
    Dictionary<TPos, Stayable> GetPositions<TPos>(TPos tpos, bool isAttack) where TPos : struct;
} 