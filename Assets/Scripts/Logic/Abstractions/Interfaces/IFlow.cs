#nullable enable

using System;

public interface IFlow : IDisposable
{
    IResult<IPiece> AddPiece<TRoom, TPos>(PieceDefinition definition, TPos position) where TRoom : IRoom where TPos : struct;
    IResult MovePiece(IPiece piece, Square target);
    IResult MeleeAttack(IPiece attacker, Square target);
    IResult ChangeRoom<TRoomFrom, TRoomTo, TPosFrom, TPosTo>(IdOfPlayer owner, TPosFrom fromPosition, TPosTo toPosition) where TRoomFrom : IRoom where TRoomTo : IRoom where TPosFrom : struct where TPosTo : struct;
}