#nullable enable

using System.Collections.Immutable;
using NUnit.Framework;

public sealed class CanAttackTests
{
    private static ISequenceEnumerator<Square> MakeAttackable(params Square[] attackCells)
    {
        var move = new Pattern<Square>(new Square { X = 1, Y = 0 })
            .GetEnumerator(Square.Zero);

        var attackBuilder = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        attackBuilder[Square.Zero] = Stayable.NotStay;
        foreach (var cell in attackCells)
            attackBuilder[cell] = Stayable.Stay;

        var attack = new Pattern<Square>(attackBuilder.ToImmutable(), guarantee: true)
            .GetEnumerator(Square.Zero);

        return new CombinedEnumerator(move, attack);
    }

    [Test]
    public void Squares_ContainsAllPositions_AndPropagatesAttackable()
    {
        var e = MakeAttackable(new Square { X = 1, Y = 0 });

        Assert.That(e.Squares, Has.Count.EqualTo(e.PositionsAll.Count));

        var target = new Square { X = 1, Y = 0 };
        Assert.That(e.Squares[target].IsAttackable, Is.True);
        Assert.That(e.Squares[Square.Zero].IsAttackable, Is.False);
    }

    [Test]
    public void Squares_AreConsistentWithPositions()
    {
        var e = MakeAttackable(new Square { X = 1, Y = 0 });

        foreach (var kv in e.Squares)
        {
            Assert.That(e.PositionsAll, Does.ContainKey(kv.Key));
            Assert.That(e.PositionsAll[kv.Key], Is.EqualTo(kv.Value.Stayable));
        }
    }

    [Test]
    public void CanAttack_TrueForAttackableCell()
    {
        var e = MakeAttackable(new Square { X = 1, Y = 0 });
        Assert.That(e.CanAttack(new Square { X = 1, Y = 0 }), Is.True);
    }

    [Test]
    public void CanAttack_FalseForNonAttackableCell()
    {
        var e = MakeAttackable(new Square { X = 1, Y = 0 });
        Assert.That(e.CanAttack(new Square { X = 0, Y = 1 }), Is.False);
    }

    [Test]
    public void CanAttack_TrueForSingleStepAttackPath()
    {
        var e = MakeAttackable(new Square { X = 1, Y = 0 });

        var path = e.CombinePositionsIntoPath(new[]
        {
            Square.Zero, new Square { X = 1, Y = 0 }
        });

        Assert.That(e.CanAttack(path), Is.True);
    }

    [Test]
    public void CanAttack_False_IfIntermediateCellIsNotAttackable()
    {
        var move = new Pattern<Square>(new Square { X = 1, Y = 0 })
            .WithAdded(new Square { X = 2, Y = 0 }, Stayable.Stay)
            .GetEnumerator(Square.Zero);

        var attackBuilder = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        attackBuilder[Square.Zero] = Stayable.NotStay;
        attackBuilder[new Square { X = 1, Y = 0 }] = Stayable.NotStay;
        attackBuilder[new Square { X = 2, Y = 0 }] = Stayable.Stay;

        var attack = new Pattern<Square>(attackBuilder.ToImmutable(), guarantee: true)
            .GetEnumerator(Square.Zero);

        ISequenceEnumerator<Square> combined = new CombinedEnumerator(move, attack);

        var path = combined.CombinePositionsIntoPath(new[]
        {
        Square.Zero,
        new Square { X = 1, Y = 0 },
        new Square { X = 2, Y = 0 }
    });

        Assert.That(combined.CanAttack(path), Is.False);
    }

    [Test]
    public void StayAndAttackSquares_AreConsistent()
    {
        var e = MakeAttackable(new Square { X = 1, Y = 0 });

        Assert.That(e.StaySquares, Is.EquivalentTo(e.PositionsStayables));
        Assert.That(e.AttackSquares, Is.EquivalentTo(e.PositionsAttackable.Keys));
    }
}