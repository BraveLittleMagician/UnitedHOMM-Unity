#nullable enable

using NUnit.Framework;

public class MovementBoundsTests
{
    private const int _size = 8;

    private static (Board board, LoggerNull logger, EventBus bus, ModifierHost host) Setup()
    {
        var logger = new LoggerNull();
        var bus = new EventBus(logger);
        var host = new ModifierHost(bus, logger);
        var seats = new Seats(2, 1);
        var board = new Board(seats, new Square { X = _size, Y = _size, Z = 1, W = 1 }, wwOffset: 0, bus, logger);
        return (board, logger, bus, host);
    }

    private static IPiece CreatePiece(
        Board board, EventBus bus, ModifierHost host, LoggerNull logger,
        IMovementFactory movementFactory, Square pos)
    {
        var factory = new PieceFactory(bus, host, logger);
        var def = new PieceDefinition(
            "Test", new IdOfPlayer(0, 0), 10, 1, 1,
            FacingDirection.PlusY,
            new[] { movementFactory });
        var piece = factory.Create(0, def);
        board.Add(piece, pos, false, out _);
        return piece;
    }

    [Test]
    public void Rook_CornerShape_AllCellsInsideBoard()
    {
        var (board, logger, bus, host) = Setup();
        var rook = CreatePiece(board, bus, host, logger,
            new MovementFactory<PathSetPattern>(new PathSetPattern(RookRules.Create(), 8)),
            new Square { X = 0, Y = 0, Z = 0 });

        var shape = rook.Movements[0].GetShape(rook, new Square { X = 0, Y = 0, Z = 0 }, board, includeOccupied: false);

        Assert.IsNotEmpty(shape);
        foreach (var sc in shape)
            Assert.IsTrue(board.IsInside(sc.Cell), $"Rook from (0,0) returned cell outside board: {sc.Cell}");
    }

    [Test]
    public void Rook_CornerShape_HasOnlySevenInEachDirection()
    {
        var (board, logger, bus, host) = Setup();
        var rook = CreatePiece(board, bus, host, logger,
            new MovementFactory<PathSetPattern>(new PathSetPattern(RookRules.Create(), 8)),
            new Square { X = 0, Y = 0, Z = 0 });

        var shape = rook.Movements[0].GetShape<Square>(rook, new Square { X = 0, Y = 0, Z = 0 }, board, includeOccupied: false);
        int stayCount = 0;
        foreach (var sc in shape) if (sc.Stayable == Stayable.Stay) stayCount++;
        Assert.AreEqual(14, stayCount, "Rook at (0,0) should have 14 reachable cells (7 right + 7 up)");
    }

    [Test]
    public void King_CornerShape_AllCellsInsideBoard()
    {
        var (board, logger, bus, host) = Setup();
        var king = CreatePiece(board, bus, host, logger,
            new MovementFactory<RelativeArea<Square>>(new RelativeArea<Square>(MultipleAxes.Two, isCircle: false)),
            new Square { X = 0, Y = 0, Z = 0 });

        var shape = king.Movements[0].GetShape<Square>(king, new Square { X = 0, Y = 0, Z = 0 }, board, includeOccupied: false);

        foreach (var sc in shape)
            Assert.IsTrue(board.IsInside(sc.Cell), $"King from (0,0) returned cell outside board: {sc.Cell}");
    }

    [Test]
    public void King_CornerShape_HasThreeValidCells()
    {
        var (board, logger, bus, host) = Setup();
        var king = CreatePiece(board, bus, host, logger,
            new MovementFactory<RelativeArea<Square>>(new RelativeArea<Square>(MultipleAxes.Two, isCircle: false)),
            new Square { X = 0, Y = 0, Z = 0 });

        var shape = king.Movements[0].GetShape<Square>(king, new Square { X = 0, Y = 0, Z = 0 }, board, includeOccupied: false);

        // Из (0,0) King может идти только на (0,1), (1,1), (1,0)
        Assert.AreEqual(3, shape.Count);
        CollectionAssert.AreEquivalent(
            new[]
            {
                new Square { X = 0, Y = 1, Z = 0 },
                new Square { X = 1, Y = 1, Z = 0 },
                new Square { X = 1, Y = 0, Z = 0 },
            },
            (System.Collections.Generic.IEnumerable<Square>)GetCells(shape));
    }

    [Test]
    public void GetValidMoves_Corner_DoesNotContainOutOfBounds()
    {
        var (board, logger, bus, host) = Setup();
        var rook = CreatePiece(board, bus, host, logger,
            new MovementFactory<PathSetPattern>(new PathSetPattern(RookRules.Create(), 8)),
            new Square { X = 0, Y = 0, Z = 0 });

        var validMoves = rook.Movements[0].GetValidMoves<Square>(rook, new Square { X = 0, Y = 0, Z = 0 }, board);

        foreach (var cell in validMoves)
            Assert.IsTrue(board.IsInside(cell), $"GetValidMoves returned cell outside board: {cell}");
    }

    [Test]
    public void TryFindPath_ToOutOfBounds_ReturnsFalse()
    {
        var (board, logger, bus, host) = Setup();
        var rook = CreatePiece(board, bus, host, logger,
            new MovementFactory<PathSetPattern>(new PathSetPattern(RookRules.Create(), 8)),
            new Square { X = 0, Y = 0, Z = 0 });

        bool found = rook.Movements[0].TryFindPath<Square>(
            rook,
            new Square { X = 0, Y = 0, Z = 0 },
            new Square { X = -1, Y = 0, Z = 0 },
            board,
            out _);

        Assert.IsFalse(found, "TryFindPath to out-of-bounds target must return false");
    }

    private static System.Collections.Generic.IEnumerable<Square> GetCells(System.Collections.Generic.IReadOnlyList<ShapeCell> shape)
    {
        foreach (var sc in shape) yield return sc.Cell;
    }
}