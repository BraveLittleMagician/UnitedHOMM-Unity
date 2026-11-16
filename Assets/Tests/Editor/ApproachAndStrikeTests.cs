#nullable enable

using NUnit.Framework;

public class ApproachAndStrikeTests
{
    private sealed class Fixture
    {
        public House House = null!;
        public Board Board = null!;
        public Registry Registry = null!;
        public Flow Flow = null!;
    }

    private static Fixture Setup()
    {
        var logger = new LoggerNull();
        var bus = new EventBus(logger);
        var host = new ModifierHost(bus, logger);
        var seats = new Seats(2, 1);
        var pieceFactory = new PieceFactory(bus, host, logger);
        var house = new House(seats, bus, logger, pieceFactory);
        var registry = new Registry(house);
        var board = new Board(seats, new Square { X = 8, Y = 8, Z = 1, W = 1 }, wwOffset: 0, bus, logger);
        house.AddRoom(board);
        host.Registry = registry;

        var mv = new MovementValidator(logger);
        var ab = new AbilityService(bus, logger);
        var atp = new AttackTargetProvider(host);
        var cs = new CombatService(logger, bus, atp, host);
        var flow = new Flow(house, registry, bus, logger, mv, ab, cs, host);

        return new Fixture { House = house, Board = board, Registry = registry, Flow = flow };
    }

    private static IPiece CreateQueen(Fixture f, IdOfPlayer owner, Square pos) =>
        f.Flow.AddPiece<Board, Square>(new PieceDefinition(
            "Queen", owner, 20, 1, 1, FacingDirection.PlusY,
            new[] { new MovementFactory<PathSetPattern>(new PathSetPattern(QueenRules.Create(), 8)) }), pos).Value!;

    private static IPiece CreateKing(Fixture f, IdOfPlayer owner, Square pos) =>
        f.Flow.AddPiece<Board, Square>(new PieceDefinition(
            "King", owner, 25, 1, 1, FacingDirection.PlusY,
            new[] { new MovementFactory<RelativeArea<Square>>(
            new RelativeArea<Square>(MultipleAxes.Two, isCircle: false),
            new RelativeArea<Square>(MultipleAxes.Two, isCircle: false)) }), pos).Value!;

    private static IPiece CreatePawn(Fixture f, IdOfPlayer owner, Square pos, int hp, FacingDirection facing) =>
        f.Flow.AddPiece<Board, Square>(new PieceDefinition(
            "Pawn", owner, hp, 1, 1, facing,
            new[] { new MovementFactory<Pattern<Square>>(PawnRules.CreateMoveShape(), PawnRules.CreateAttackShape()) }), pos).Value!;

    [Test]
    public void Queen_ApproachesAdjacent_ThenStrikes()
    {
        var f = Setup();
        var white = new IdOfPlayer(0, 0);
        var black = new IdOfPlayer(1, 0);

        var queen = CreateQueen(f, white, new Square { X = 3, Y = 0, Z = 0 });
        var pawn = CreatePawn(f, black, new Square { X = 3, Y = 6, Z = 0 }, hp: 10, FacingDirection.MinusY);

        var result = f.Flow.MeleeAttack(queen, new Square { X = 3, Y = 6, Z = 0 });

        Assert.IsTrue(result.IsSuccess, result.Error);
        Assert.IsTrue(f.Board.TryToGetPosition<Square>(queen.IndexInHouse, out var queenPos));
        Assert.AreEqual(new Square { X = 3, Y = 5, Z = 0 }, queenPos,
            "Queen must move to the cell adjacent to target before striking");
        Assert.IsFalse(pawn.IsDead);
        Assert.AreEqual(9, pawn.Health);
        Assert.IsTrue(f.Board.TryToGetPosition<Square>(pawn.IndexInHouse, out var pawnPos));
        Assert.AreEqual(new Square { X = 3, Y = 6, Z = 0 }, pawnPos);
    }

    [Test]
    public void Queen_ApproachesAndKills_ThenJumpsToTargetCell()
    {
        var f = Setup();
        var white = new IdOfPlayer(0, 0);
        var black = new IdOfPlayer(1, 0);

        var queen = CreateQueen(f, white, new Square { X = 3, Y = 0, Z = 0 });
        var pawn = CreatePawn(f, black, new Square { X = 3, Y = 6, Z = 0 }, hp: 1, FacingDirection.MinusY);

        var result = f.Flow.MeleeAttack(queen, new Square { X = 3, Y = 6, Z = 0 });

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(pawn.IsDead);
        Assert.IsTrue(f.Board.TryToGetPosition<Square>(queen.IndexInHouse, out var queenPos));
        Assert.AreEqual(new Square { X = 3, Y = 6, Z = 0 }, queenPos,
            "Queen must jump to target's cell after kill");
    }

    [Test]
    public void King_AlreadyAdjacent_DoesNotMove()
    {
        var f = Setup();
        var white = new IdOfPlayer(0, 0);
        var black = new IdOfPlayer(1, 0);

        var king = CreateKing(f, white, new Square { X = 4, Y = 4, Z = 0 });
        var pawn = CreatePawn(f, black, new Square { X = 5, Y = 4, Z = 0 }, hp: 10, FacingDirection.MinusY);

        var result = f.Flow.MeleeAttack(king, new Square { X = 5, Y = 4, Z = 0 });

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(f.Board.TryToGetPosition<Square>(king.IndexInHouse, out var kingPos));
        Assert.AreEqual(new Square { X = 4, Y = 4, Z = 0 }, kingPos,
            "King already adjacent — must not move");
        Assert.AreEqual(9, pawn.Health);
    }

    [Test]
    public void Pawn_DiagonalAttack_DoesNotMove()
    {
        var f = Setup();
        var white = new IdOfPlayer(0, 0);
        var black = new IdOfPlayer(1, 0);

        var pawn = CreatePawn(f, white, new Square { X = 3, Y = 1, Z = 0 }, hp: 10, FacingDirection.PlusY);
        var enemy = CreatePawn(f, black, new Square { X = 2, Y = 2, Z = 0 }, hp: 10, FacingDirection.MinusY);

        var result = f.Flow.MeleeAttack(pawn, new Square { X = 2, Y = 2, Z = 0 });

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(f.Board.TryToGetPosition<Square>(pawn.IndexInHouse, out var pawnPos));
        Assert.AreEqual(new Square { X = 3, Y = 1, Z = 0 }, pawnPos,
            "Pawn already attacks diagonally — must not move");
        Assert.AreEqual(9, enemy.Health);
    }

    [Test]
    public void Approach_ThenUndo_RestoresBothPositions()
    {
        var f = Setup();
        var white = new IdOfPlayer(0, 0);
        var black = new IdOfPlayer(1, 0);

        var queen = CreateQueen(f, white, new Square { X = 3, Y = 0, Z = 0 });
        var pawn = CreatePawn(f, black, new Square { X = 3, Y = 6, Z = 0 }, hp: 10, FacingDirection.MinusY);

        f.Flow.MeleeAttack(queen, new Square { X = 3, Y = 6, Z = 0 });
        var undo = f.Flow.Undo();
        Assert.IsTrue(undo.IsSuccess);

        Assert.IsTrue(f.Board.TryToGetPosition<Square>(queen.IndexInHouse, out var queenPos));
        Assert.AreEqual(new Square { X = 3, Y = 0, Z = 0 }, queenPos, "Undo restores Queen to origin");
        Assert.AreEqual(10, pawn.Health, "Undo restores Pawn HP");
    }

    [Test]
    public void Approach_RollbackOnAttackFailure_RestoresPosition()
    {
        // Атакующий без charges — approach должен откатиться. Проверим, что Queen
        // не сдвинулся, если атака провалилась уже после Displace.
        var f = Setup();
        var white = new IdOfPlayer(0, 0);
        var black = new IdOfPlayer(1, 0);

        var queen = CreateQueen(f, white, new Square { X = 3, Y = 0, Z = 0 });
        var pawn = CreatePawn(f, black, new Square { X = 3, Y = 6, Z = 0 }, hp: 10, FacingDirection.MinusY);

        // Обнуляем charges
        queen.TryConsumeAttackCharge();

        var result = f.Flow.MeleeAttack(queen, new Square { X = 3, Y = 6, Z = 0 });
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(f.Board.TryToGetPosition<Square>(queen.IndexInHouse, out var queenPos));
        Assert.AreEqual(new Square { X = 3, Y = 0, Z = 0 }, queenPos,
            "Approach must roll back if the attack fails");
    }
}