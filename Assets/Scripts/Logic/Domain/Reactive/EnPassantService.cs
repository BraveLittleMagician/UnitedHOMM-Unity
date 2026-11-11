#nullable enable
using System;

public sealed class EnPassantService : ReactiveActionService<DoubleMoveRecord, EnPassantTarget>
{
    public EnPassantService(IRegistry registry, TurnManager turnManager, IEventBus eventBus, ModifierHost modifierHost)
        : base(registry, turnManager, eventBus, modifierHost) { }

    protected override void RegisterHandlers()
    {
        EventBus.Subscribe<PieceStartedActionEvent>(OnPieceStartedAction);
    }

    protected override void UnregisterHandlers()
    {
        EventBus.Unsubscribe<PieceStartedActionEvent>(OnPieceStartedAction);
    }

    protected override IdOfPlayer GetValidFor(DoubleMoveRecord record) => record.ValidFor;

    protected override bool PassesBaseCheck(IPiece attacker)
    {
        if (attacker.IsDead) return false;
        if (attacker.HasMovedThisTurn) return false;
        if (attacker.HasPromotedThisTurn) return false;
        if (attacker.AttackCharges <= 0) return false;
        return ModifierHost.TryGetModifier<EnPassantModifier>(attacker.IndexInHouse, out var ep)
            && ep.CurrentCharges > 0;
    }

    protected override bool IsValidRecord(DoubleMoveRecord record, IPiece attacker)
    {
        if (!Registry.TryToGetPiece(record.MoverIndex, out var victim) || victim == null) return false;
        if (victim.IsDead) return false;
        if (victim.Owner.Equals(attacker.Owner)) return false;

        if (!Registry.TryToGetRoom(victim.IndexInHouse, out var victimRoom) || !(victimRoom is ISquarePositionRoom vSq)) return false;
        if (!vSq.TryToGetPosition<Square>(victim.IndexInHouse, out var victimPos)) return false;
        if (!victimPos.Equals(record.To)) return false;

        if (!Registry.TryToGetRoom(attacker.IndexInHouse, out var attackerRoom) || !(attackerRoom is ISquarePositionRoom aSq)) return false;
        if (!aSq.TryToGetPosition<Square>(attacker.IndexInHouse, out var attackerPos)) return false;

        return IsPerpendicularNeighbor(attackerPos, record.To, victim.Facing);
    }

    protected override EnPassantTarget BuildTarget(DoubleMoveRecord record, IPiece attacker)
    {
        Registry.TryToGetPiece(record.MoverIndex, out var victim);
        return new EnPassantTarget(victim!, record.To, record.Via);
    }

    private void OnPieceStartedAction(PieceStartedActionEvent e)
    {
        if (e.Activity is not MoveActivity move) return;
        if (e.Piece.IsDead) return;

        var dir = FacingToSquareDelta(e.Piece.Facing);
        if (dir.IsZero) return;

        var delta = move.To.Subtract(move.From);
        var expectedDelta = dir.Add(dir);
        if (!delta.Equals(expectedDelta)) return;

        var via = move.From.Add(dir);
        var validFor = GetNextPlayer(e.Piece.Owner);
        if (validFor.Equals(e.Piece.Owner)) return;

        AddRecord(new DoubleMoveRecord(e.Piece.IndexInHouse, move.From, via, move.To, validFor));
    }

    private IdOfPlayer GetNextPlayer(IdOfPlayer current)
    {
        var order = TurnManager.Order;
        for (int i = 0; i < order.Count; i++)
            if (order[i].Equals(current))
                return order[(i + 1) % order.Count];
        return current;
    }

    private static Square FacingToSquareDelta(FacingDirection facing) => facing switch
    {
        FacingDirection.PlusX => new Square { X = 1 },
        FacingDirection.MinusX => new Square { X = -1 },
        FacingDirection.PlusY => new Square { Y = 1 },
        FacingDirection.MinusY => new Square { Y = -1 },
        FacingDirection.PlusZ => new Square { Z = 1 },
        FacingDirection.MinusZ => new Square { Z = -1 },
        _ => Square.Zero,
    };

    private static bool IsPerpendicularNeighbor(Square attackerPos, Square victimPos, FacingDirection victimFacing)
    {
        var dx = victimPos.X - attackerPos.X;
        var dy = victimPos.Y - attackerPos.Y;
        var dz = victimPos.Z - attackerPos.Z;
        var dw = victimPos.W - attackerPos.W;

        int ax = Math.Abs(dx), ay = Math.Abs(dy), az = Math.Abs(dz), aw = Math.Abs(dw);
        if (ax + ay + az + aw != 1) return false;

        return victimFacing switch
        {
            FacingDirection.PlusX or FacingDirection.MinusX => ax == 0,
            FacingDirection.PlusY or FacingDirection.MinusY => ay == 0,
            FacingDirection.PlusZ or FacingDirection.MinusZ => az == 0,
            _ => false,
        };
    }
}