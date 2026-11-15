#nullable enable

using NUnit.Framework;

public class ConfigOfBoardWWOffsetTests
{
    [TestCase(1, null, 0)]
    [TestCase(1, true, 0)]
    [TestCase(1, false, 0)]
    [TestCase(2, null, 0)]   // null трактуется как false
    [TestCase(2, false, 0)]
    [TestCase(2, true, -1)]
    [TestCase(3, null, -1)]
    [TestCase(3, true, -1)]
    [TestCase(3, false, -1)]
    public void WWOffset_MatchesTable(int w, bool? signWNegative, int expected)
    {
        var config = new ConfigOfBoard
        {
            Axes = MultipleAxesFromTwo.Four,
            FieldSize = 8,
            W = w,
            SignWNegative = signWNegative,
        };

        Assert.AreEqual(expected, config.WWOffset);
    }

    [Test]
    public void BoardSizeFactory_W1_ProducesSingleLayer()
    {
        var config = new ConfigOfBoard { Axes = MultipleAxesFromTwo.Four, FieldSize = 8, W = 1 };
        var size = BoardSizeFactory.FromConfig(config);
        Assert.AreEqual(1, size.W);
    }

    [Test]
    public void BoardSizeFactory_W2_ProducesTwoLayers()
    {
        var config = new ConfigOfBoard { Axes = MultipleAxesFromTwo.Four, FieldSize = 8, W = 2 };
        var size = BoardSizeFactory.FromConfig(config);
        Assert.AreEqual(2, size.W);
    }

    [Test]
    public void BoardSizeFactory_W3_ProducesThreeLayers()
    {
        var config = new ConfigOfBoard { Axes = MultipleAxesFromTwo.Four, FieldSize = 8, W = 3 };
        var size = BoardSizeFactory.FromConfig(config);
        Assert.AreEqual(3, size.W);
    }

    [Test]
    public void BoardSizeFactory_NonFourAxes_ForcesW1()
    {
        var config2D = new ConfigOfBoard { Axes = MultipleAxesFromTwo.Two, FieldSize = 8, W = 3 };
        var size2D = BoardSizeFactory.FromConfig(config2D);
        Assert.AreEqual(1, size2D.W);
        Assert.AreEqual(1, size2D.Z);
    }
}