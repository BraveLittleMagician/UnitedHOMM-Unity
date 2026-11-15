#nullable enable

using NUnit.Framework;

public class BoardIsInsideTests
{
    private static Board CreateBoard(Square size, int wwOffset)
    {
        var logger = new LoggerNull();
        var bus = new EventBus(logger);
        var seats = new Seats(2, 1);
        return new Board(seats, size, wwOffset, bus, logger);
    }

    [Test]
    public void Board_InsideCheck_RespectsXYZBounds()
    {
        var board = CreateBoard(new Square { X = 8, Y = 8, Z = 1, W = 1 }, wwOffset: 0);

        Assert.IsTrue(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = 0 }));
        Assert.IsTrue(board.IsInside(new Square { X = 7, Y = 7, Z = 0, W = 0 }));
        Assert.IsFalse(board.IsInside(new Square { X = 8, Y = 0, Z = 0, W = 0 }));  // X вне
        Assert.IsFalse(board.IsInside(new Square { X = 0, Y = 8, Z = 0, W = 0 }));  // Y вне
        Assert.IsFalse(board.IsInside(new Square { X = -1, Y = 0, Z = 0, W = 0 })); // X отрицательный
    }

    [Test]
    public void Board_W1_OnlyW0IsValid()
    {
        var board = CreateBoard(new Square { X = 8, Y = 8, Z = 1, W = 1 }, wwOffset: 0);

        Assert.IsTrue(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = 0 }));
        Assert.IsFalse(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = -1 }));
        Assert.IsFalse(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = 1 }));
    }

    [Test]
    public void Board_W2_SignWNegative_True_AllowsMinusOneAndZero()
    {
        var board = CreateBoard(new Square { X = 8, Y = 8, Z = 1, W = 2 }, wwOffset: -1);

        Assert.IsTrue(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = -1 }));
        Assert.IsTrue(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = 0 }));
        Assert.IsFalse(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = 1 }));
    }

    [Test]
    public void Board_W2_SignWNegative_False_AllowsZeroAndPlusOne()
    {
        var board = CreateBoard(new Square { X = 8, Y = 8, Z = 1, W = 2 }, wwOffset: 0);

        Assert.IsTrue(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = 0 }));
        Assert.IsTrue(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = 1 }));
        Assert.IsFalse(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = -1 }));
    }

    [Test]
    public void Board_W3_Symmetric_AllowsMinusOneZeroPlusOne()
    {
        var board = CreateBoard(new Square { X = 8, Y = 8, Z = 1, W = 3 }, wwOffset: -1);

        Assert.IsTrue(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = -1 }));
        Assert.IsTrue(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = 0 }));
        Assert.IsTrue(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = 1 }));
        Assert.IsFalse(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = 2 }));
        Assert.IsFalse(board.IsInside(new Square { X = 0, Y = 0, Z = 0, W = -2 }));
    }
}
