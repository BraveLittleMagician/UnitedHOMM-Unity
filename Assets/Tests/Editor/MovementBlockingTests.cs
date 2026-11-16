#nullable enable

using NUnit.Framework;
using System.Linq;

public class MovementBlockingTests
{
    private static (Board board, EventBus bus, ModifierHost host, LoggerNull logger) Setup()
    {
        var logger = new LoggerNull();
        var bus = new EventBus(logger);
        var host = new ModifierHost(bus, logger);
        var seats = new Seats(2, 1);
        var board = new Board(seats, new Square { X = 8, Y = 8, Z = 1, W = 1 }, wwOffset: 0, bus, logger);
        return (board, bus, host, logger);
    }

    private static IPiece CreatePiece(Board board, EventBus bus, ModifierHost host, LoggerNull logger, string name, IdOfPlayer owner, IMovementFactory factory, Square pos)
    {
        var pieceFactory = new PieceFactory(bus, host, logger);
        var def = new PieceDefinition(name, owner, 10, 1, 1, FacingDirection.PlusY, new[] { factory });
        var piece = pieceFactory.Create(0, def);
        board.Add(piece, pos, false, out _);
        return piece;
    }

    private static IMovementFactory RookMovement() =>
        new MovementFactory<PathSetPattern>(new PathSetPattern(RookRules.Create(), 8));

    [Test]
    public void Rook_AllyBlocksMovement_MarkersDoNotAppearBeyond()
    {
        var (board, bus, host, logger) = Setup();
        var white = new IdOfPlayer(0, 0);
        var black = new IdOfPlayer(1, 0);

        var rook = CreatePiece(board, bus, host, logger, "Rook", white, RookMovement(),
            new Square { X = 3, Y = 0, Z = 0 });
        CreatePiece(board, bus, host, logger, "Ally", white,
            new MovementFactory<Pattern<Square>>(PawnRules.CreateMoveShape()), new Square { X = 3, Y = 1, Z = 0 });

        var shape = rook.Movements[0].GetShape(rook, new Square { X = 3, Y = 0, Z = 0 }, board, includeOccupied: false);

        // За союзником (3,2), (3,3), ... — маркеров быть не должно.
        Assert.IsFalse(shape.Any(sc => sc.Cell == new Square { X = 3, Y = 2, Z = 0 }),
            "Marker behind ally must not appear");
        Assert.IsFalse(shape.Any(sc => sc.Cell == new Square { X = 3, Y = 1, Z = 0 }),
            "Marker on ally cell must not appear");
    }

    [Test]
    public void Rook_EnemyBlocksAttack_OnlyFirstEnemyIsAttackable()
    {
        var (board, bus, host, logger) = Setup();
        var white = new IdOfPlayer(0, 0);
        var black = new IdOfPlayer(1, 0);

        var rook = CreatePiece(board, bus, host, logger, "Rook", white, RookMovement(),
            new Square { X = 3, Y = 0, Z = 0 });
        CreatePiece(board, bus, host, logger, "EnemyPawn", black,
            new MovementFactory<Pattern<Square>>(PawnRules.CreateMoveShape()), new Square { X = 3, Y = 6, Z = 0 });
        CreatePiece(board, bus, host, logger, "EnemyQueen", black,
            new MovementFactory<Pattern<Square>>(PawnRules.CreateMoveShape()), new Square { X = 3, Y = 7, Z = 0 });

        var shape = rook.Movements[0].GetShape(rook, new Square { X = 3, Y = 0, Z = 0 }, board, includeOccupied: true);

        Assert.IsTrue(shape.Any(sc => sc.Cell == new Square { X = 3, Y = 6, Z = 0 } && sc.Attackable == Attackable.Attack),
            "First enemy must be attackable");
        Assert.IsFalse(shape.Any(sc => sc.Cell == new Square { X = 3, Y = 7, Z = 0 }),
            "Enemy behind first enemy must not appear in shape");
    }

    [Test]
    public void Rook_AllyBlocksAttack_AllyIsNotAttackable()
    {
        var (board, bus, host, logger) = Setup();
        var white = new IdOfPlayer(0, 0);

        var rook = CreatePiece(board, bus, host, logger, "Rook", white, RookMovement(),
            new Square { X = 3, Y = 0, Z = 0 });
        CreatePiece(board, bus, host, logger, "Ally", white,
            new MovementFactory<Pattern<Square>>(PawnRules.CreateMoveShape()), new Square { X = 3, Y = 4, Z = 0 });

        var shape = rook.Movements[0].GetShape(rook, new Square { X = 3, Y = 0, Z = 0 }, board, includeOccupied: true);

        Assert.IsFalse(shape.Any(sc => sc.Cell == new Square { X = 3, Y = 4, Z = 0 }),
            "Ally must not be in shape");
        Assert.IsFalse(shape.Any(sc => sc.Cell == new Square { X = 3, Y = 5, Z = 0 }),
            "Cell behind ally must not appear");
    }

    [Test]
    public void Rook_NoObstacles_AttackReachesToEndOfLine()
    {
        var (board, bus, host, logger) = Setup();
        var white = new IdOfPlayer(0, 0);
        var black = new IdOfPlayer(1, 0);

        var rook = CreatePiece(board, bus, host, logger, "Rook", white, RookMovement(),
            new Square { X = 3, Y = 0, Z = 0 });
        CreatePiece(board, bus, host, logger, "EnemyQueen", black,
            new MovementFactory<Pattern<Square>>(PawnRules.CreateMoveShape()), new Square { X = 3, Y = 7, Z = 0 });

        var shape = rook.Movements[0].GetShape(rook, new Square { X = 3, Y = 0, Z = 0 }, board, includeOccupied: true);

        Assert.IsTrue(shape.Any(sc => sc.Cell == new Square { X = 3, Y = 7, Z = 0 } && sc.Attackable == Attackable.Attack),
            "Enemy at far end of empty line must be attackable");
    }

    [Test]
    public void Rook_GetValidMoves_StopsAtAlly()
    {
        var (board, bus, host, logger) = Setup();
        var white = new IdOfPlayer(0, 0);

        var rook = CreatePiece(board, bus, host, logger, "Rook", white, RookMovement(),
            new Square { X = 3, Y = 0, Z = 0 });
        CreatePiece(board, bus, host, logger, "Ally", white,
            new MovementFactory<Pattern<Square>>(PawnRules.CreateMoveShape()), new Square { X = 3, Y = 4, Z = 0 });

        var validMoves = rook.Movements[0].GetValidMoves(rook, new Square { X = 3, Y = 0, Z = 0 }, board);

        Assert.IsTrue(validMoves.Contains(new Square { X = 3, Y = 3, Z = 0 }));
        Assert.IsFalse(validMoves.Contains(new Square { X = 3, Y = 4, Z = 0 }), "Ally cell must not be valid move");
        Assert.IsFalse(validMoves.Contains(new Square { X = 3, Y = 5, Z = 0 }), "Cell behind ally must not be valid move");
    }

    [Test]
    public void King_SurroundedByAllies_NoValidMoves()
    {
        var (board, bus, host, logger) = Setup();
        var white = new IdOfPlayer(0, 0);

        var king = CreatePiece(board, bus, host, logger, "King", white,
            new MovementFactory<RelativeArea<Square>>(new RelativeArea<Square>(MultipleAxes.Two, isCircle: false)),
            new Square { X = 4, Y = 4, Z = 0 });

        var offsets = new (int dx, int dy)[]
        {
            (-1,-1),(-1,0),(-1,1),(0,-1),(0,1),(1,-1),(1,0),(1,1),
        };
        foreach (var (dx, dy) in offsets)
        {
            CreatePiece(board, bus, host, logger, "Ally", white,
                new MovementFactory<Pattern<Square>>(PawnRules.CreateMoveShape()),
                new Square { X = 4 + dx, Y = 4 + dy, Z = 0 });
        }

        var validMoves = king.Movements[0].GetValidMoves<Square>(king, new Square { X = 4, Y = 4, Z = 0 }, board);
        Assert.IsEmpty(validMoves, "King surrounded by allies must have no valid moves");
    }
}