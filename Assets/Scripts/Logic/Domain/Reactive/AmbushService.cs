#nullable enable
using System;

public sealed class AmbushService : ReactiveActionService<AmbushRecord, AmbushTarget>
{
    public AmbushService(IRegistry registry, TurnManager turnManager, IEventBus eventBus, ModifierHost modifierHost)
        : base(registry, turnManager, eventBus, modifierHost) { }

    protected override void RegisterHandlers()
    {
        EventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
    }

    protected override void UnregisterHandlers()
    {
        EventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved);
    }

    protected override IdOfPlayer GetValidFor(AmbushRecord record) => record.ValidFor;

    protected override bool PassesBaseCheck(IPiece attacker)
    {
        if (attacker.IsDead) return false;
        if (attacker.HasPromotedThisTurn) return false;
        return ModifierHost.TryGetModifier<AmbushModifier>(attacker.IndexInHouse, out var amb)
            && amb.CurrentCharges > 0;
    }

    protected override bool IsValidRecord(AmbushRecord record, IPiece attacker)
    {
        if (record.AttackerIndex != attacker.IndexInHouse) return false;
        if (!Registry.TryToGetPiece(record.VictimIndex, out var victim) || victim == null) return false;
        if (victim.IsDead) return false;
        if (victim.Owner.Equals(attacker.Owner)) return false;

        if (!Registry.TryToGetRoom(victim.IndexInHouse, out var victimRoom) || !(victimRoom is ISquarePositionRoom vSq)) return false;
        if (!vSq.TryToGetPosition<Square>(victim.IndexInHouse, out var victimPos)) return false;
        if (!victimPos.Equals(record.VictimCell)) return false;

        if (!Registry.TryToGetRoom(attacker.IndexInHouse, out var attackerRoom) || !(attackerRoom is ISquarePositionRoom aSq)) return false;
        if (!aSq.TryToGetPosition<Square>(attacker.IndexInHouse, out var attackerPos)) return false;

        return IsChebyshev1(attackerPos, victimPos);
    }

    protected override AmbushTarget BuildTarget(AmbushRecord record, IPiece attacker)
    {
        Registry.TryToGetPiece(record.VictimIndex, out var victim);
        return new AmbushTarget(victim!, record.VictimCell);
    }

    private void OnPieceMoved(PieceMovedEvent<Square> e)
    {
        if (e.Piece.IsDead) return;
        var toPos = e.ToPosition;

        foreach (var piece in Registry.AllPieces)
        {
            if (piece.IsDead) continue;
            if (piece.Owner.Equals(e.Piece.Owner)) continue;
            if (!ModifierHost.TryGetModifier<AmbushModifier>(piece.IndexInHouse, out var amb) || amb.CurrentCharges <= 0) continue;

            if (!Registry.TryToGetRoom(piece.IndexInHouse, out var ambusherRoom) || !(ambusherRoom is ISquarePositionRoom sq)) continue;
            if (!sq.TryToGetPosition<Square>(piece.IndexInHouse, out var ambusherPos)) continue;
            if (!IsChebyshev1(ambusherPos, toPos)) continue;

            AddRecord(new AmbushRecord(piece.IndexInHouse, e.Piece.IndexInHouse, toPos, piece.Owner));
        }
    }

    private static bool IsChebyshev1(Square a, Square b)
    {
        int dx = Math.Abs(a.X - b.X);
        int dy = Math.Abs(a.Y - b.Y);
        int dz = Math.Abs(a.Z - b.Z);
        int dw = Math.Abs(a.W - b.W);
        return dx <= 1 && dy <= 1 && dz <= 1 && dw <= 1 && (dx + dy + dz + dw) > 0;
    }
}
