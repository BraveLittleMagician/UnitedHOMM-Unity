#nullable enable

using System;
using System.Collections.Generic;

public abstract class Movement : IMovement, IDisposable
{
    public abstract Modifiers Modifiers { get; }
    public abstract bool CanMove<TPos>(IPiece piece, IPath<TPos> path, IReadOnlyRoom room, bool isAttack) where TPos : struct;
    public abstract bool IsApplicableToRoom(IReadOnlyRoom room);
    public abstract List<TPos> GetStayables<TPos>(TPos tpos, bool isAttack) where TPos : struct;
    public abstract Dictionary<TPos, Stayable> GetPositions<TPos>(TPos tpos, bool isAttack) where TPos : struct;
    public abstract void Dispose();
}