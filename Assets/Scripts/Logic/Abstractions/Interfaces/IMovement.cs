#nullable enable

using System;
using System.Collections.Generic;

public interface IMovement : IDisposable
{
    bool CanMove<TPos>(IPath<TPos> path) where TPos : struct;
    bool IsApplicableToRoom(IReadOnlyRoom room);
    List<TPos> GetStayables<TPos>(TPos position) where TPos : struct;
    Dictionary<TPos, Stayable> GetPositions<TPos>(TPos position) where TPos : struct;
}