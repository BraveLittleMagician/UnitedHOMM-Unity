#nullable enable
using System;
using System.Collections.Generic;

public interface IFlow : IDisposable
{
    IResult<IPiece> AddPiece<TRoom, TPos>(PieceDefinition definition, TPos position) where TRoom : IRoom where TPos : struct;
    IResult MovePiece(IPiece piece, Square target);
    IResult MeleeAttack(IPiece attacker, Square target);
    IResult ActivateModifier(IPiece piece, IActiveModifier modifier);
    IResult ActivateTargetedModifier(IPiece piece, ITargetedModifier modifier, Square targetCell);
    IReadOnlyList<EnPassantTarget> GetAvailableEnPassantTargets(IPiece attacker);
    IReadOnlyList<AmbushTarget> GetAvailableAmbushTargets(IPiece attacker);
    IResult EnPassantAttack(IPiece attacker, EnPassantTarget target);
    IResult AmbushAttack(IPiece attacker, AmbushTarget target);
    IResult ChangeRoom<TRoomFrom, TRoomTo, TPosFrom, TPosTo>(IdOfPlayer owner, TPosFrom fromPosition, TPosTo toPosition) where TRoomFrom : IRoom where TRoomTo : IRoom where TPosFrom : struct where TPosTo : struct;

    IResult Undo();
    IResult Redo();
    IResult UndoAll();
    IResult Commit();

    int UndoCount { get; }
    int RedoCount { get; }
}