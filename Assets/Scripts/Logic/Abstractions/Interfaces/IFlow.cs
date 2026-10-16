#nullable enable

using System;

public interface IFlow : IDisposable
{
    IResult<IPiece> AddPiece<TRoom, TPos>(PieceDefinition definition, TPos position) where TRoom : IRoom where TPos : struct;
    IResult MovePiece<TRoom, TPos>(IdOfPlayer owner, IPath<TPos> path) where TRoom : IRoom where TPos : struct;
    IResult MeleeAttack<TRoom, TPos>(IdOfPlayer attackerOwner, IPath<TPos> path) where TRoom : IRoom where TPos : struct;
    IResult ChangeRoom<TRoomFrom, TRoomTo, TPosFrom, TPosTo>(IdOfPlayer owner, TPosFrom fromPosition, TPosTo toPosition) where TRoomFrom : IRoom where TRoomTo : IRoom where TPosFrom : struct where TPosTo : struct;
}