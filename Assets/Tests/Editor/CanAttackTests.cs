#nullable enable

using NUnit.Framework;

public sealed class CanAttackTests
{
    [Test]
    public void Squares_ContainsAllPositions_AndPropagatesAttackable()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 }, Attackable.Attack);
        var e = p.GetEnumerator(Square.Zero);

        Assert.That(e.Squares, Has.Count.EqualTo(e.PositionsAll.Count));
        var target = new Square { X = 1, Y = 0 };
        Assert.That(e.Squares[target].IsAttackable, Is.True);
        Assert.That(e.Squares[Square.Zero].IsAttackable, Is.False);
    }

    [Test]
    public void Squares_AreConsistentWithPositions()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 }, Attackable.Attack);
        var e = p.GetEnumerator(Square.Zero);

        foreach (var kv in e.Squares)
        {
            Assert.That(e.PositionsAll, Does.ContainKey(kv.Key));
            Assert.That(e.PositionsAll[kv.Key], Is.EqualTo(kv.Value.Stayable));
        }
    }

    [Test]
    public void CanAttack_TrueForAttackableCell()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 }, Attackable.Attack);
        var e = p.GetEnumerator(Square.Zero);
        Assert.That(e.CanAttack(new Square { X = 1, Y = 0 }), Is.True);
    }

    [Test]
    public void CanAttack_FalseForNonAttackableCell()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 }, Attackable.Attack);
        var e = p.GetEnumerator(Square.Zero);
        Assert.That(e.CanAttack(new Square { X = 0, Y = 1 }), Is.False);
    }

    [Test]
    public void CanAttack_TrueForSingleStepAttackPath()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 }, Attackable.Attack);
        var e = p.GetEnumerator(Square.Zero);
        var path = e.CombinePositionsIntoPath(new[]
        {
            Square.Zero, new Square { X = 1, Y = 0 }
        });
        Assert.That(e.CanAttack(path), Is.True);
    }

    [Test]
    public void CanAttack_False_IfIntermediateCellIsNotAttackable()
    {
        // Attackable только конечная клетка, промежуточная — NotAttack.
        var b = System.Collections.Immutable.ImmutableDictionary.CreateBuilder<Square, Stayable>();
        b[Square.Zero] = Stayable.NotStay;
        b[new Square { X = 1, Y = 0 }] = Stayable.NotStay;
        b[new Square { X = 2, Y = 0 }] = Stayable.NotStay;
        var p = new Pattern<Square>(b.ToImmutable());
        p = p.WithAddedAttackable(new[] { new Square { X = 2, Y = 0 } }, Attackable.Attack);
        var e = p.GetEnumerator(Square.Zero);
        var path = e.CombinePositionsIntoPath(new[]
        {
            Square.Zero, new Square { X = 1, Y = 0 }, new Square { X = 2, Y = 0 }
        });
        Assert.That(e.CanAttack(path), Is.False);
    }

    [Test]
    public void StayAndAttackSquares_AreConsistent()
    {
        var p = new Pattern<Square>(new Square { X = 1, Y = 0 }, Attackable.Attack);
        var e = p.GetEnumerator(Square.Zero);

        Assert.That(e.StaySquares, Is.EquivalentTo(e.PositionsStayables));
        Assert.That(e.AttackSquares, Is.EquivalentTo(e.PositionsAttackable.Keys));
    }
}