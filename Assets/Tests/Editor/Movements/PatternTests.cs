#nullable enable

using System.Collections.Immutable;
using NUnit.Framework;

public sealed class PatternTests
{
    [Test]
    public void SingleCell_HasExactlyOneStayable()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var e = p.GetEnumerator(Square.Zero);

        Assert.That(e.PositionsStayables, Has.Count.EqualTo(1));
        Assert.That(e.PositionsStayables[0], Is.EqualTo(new Square { X = 1, Y = 0 }));
    }

    [Test]
    public void SingleCell_StartIsNotStayable_ButIsInPositionsAll()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var e = p.GetEnumerator(Square.Zero);

        Assert.That(e.PositionsStayables, Has.No.Member(Square.Zero));   // ← было Does.Not.Contain
        Assert.That(e.PositionsAll, Does.ContainKey(Square.Zero));
        Assert.That(e.PositionsAll[Square.Zero], Is.EqualTo(Stayable.NotStay));
    }

    [Test]
    public void SingleCell_Attackable_PropagatesToPositionsAttackable()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 }, Attackable.Attack);
        var e = p.GetEnumerator(Square.Zero);

        Assert.That(e.PositionsAttackable, Does.ContainKey(new Square { X = 1, Y = 0 }));
        Assert.That(e.PositionsAttackable[new Square { X = 1, Y = 0 }],
                    Is.EqualTo(Attackable.Attack));
    }

    [Test]
    public void SingleCell_NotAttackable_PositionsAttackableIsEmpty()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var e = p.GetEnumerator(Square.Zero);

        Assert.That(e.PositionsAttackable, Is.Empty);
    }

    [Test]
    public void King_AllEightAdjacentCellsAreStayable()
    {
        var builder = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        builder[Square.Zero] = Stayable.NotStay;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                builder[new Square { X = dx, Y = dy }] = Stayable.Stay;
            }
        var king = new Pattern<Square>(builder.ToImmutable());
        var e = king.GetEnumerator(Square.Zero);

        Assert.That(e.PositionsStayables, Has.Count.EqualTo(8));
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Assert.That(e.CanMoveTo(new Square { X = dx, Y = dy }), Is.True,
                    $"cell ({dx},{dy}) should be movable");
            }
    }

    [Test]
    public void EnumeratedFromDifferentStart_TranslatesCells()
    {
        var start = new Square { X = 5, Y = 5 };
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var e = p.GetEnumerator(start);

        Assert.That(e.PositionsStayables, Has.Count.EqualTo(1));
        Assert.That(e.PositionsStayables[0], Is.EqualTo(new Square { X = 6, Y = 5 }));
    }

    [Test]
    public void CanMoveTo_ReturnsFalse_ForCellOutsidePattern()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var e = p.GetEnumerator(Square.Zero);
        Assert.That(e.CanMoveTo(new Square { X = 0, Y = 1 }), Is.False);
    }
}
