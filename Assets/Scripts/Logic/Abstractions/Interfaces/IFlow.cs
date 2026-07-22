#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

public interface IFlow : IDisposable
{
    IResult<IPiece> AddPiece<TRoom, TPos>(PieceDefinition definition, TPos position) where TRoom : IRoom where TPos : struct;
    IResult MovePiece<TRoom, TPos>(IndexOfPlayer owner, IPath<TPos> path) where TRoom : IRoom where TPos : struct;
    IResult MeleeAttack<TRoom, TPos>(IndexOfPlayer attackerOwner, IPath<TPos> path) where TRoom : IRoom where TPos : struct;
    IResult ChangeRoom<TRoomFrom, TRoomTo, TPosFrom, TPosTo>(IndexOfPlayer owner, TPosFrom fromPosition, TPosTo toPosition) where TRoomFrom : IRoom where TRoomTo : IRoom where TPosFrom : struct where TPosTo : struct;
    bool TryToGetPiece<TRoom, TPos>([NotNullWhen(true)] TPos position, IndexOfPlayer player, out IPiece? piece) where TRoom : IRoom where TPos : struct;
}