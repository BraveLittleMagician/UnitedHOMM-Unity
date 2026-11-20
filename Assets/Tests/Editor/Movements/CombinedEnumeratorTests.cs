#nullable enable

using System.Collections.Immutable;
using NUnit.Framework;

public sealed class CombinedEnumeratorTests
{
    [Test]
    public void PositionsAll_IsUnionOfMoveAndAttack()
    {
        var moveP = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var attackP = new Pattern<Square>(new Square { X = 0, Y = 1 }, Attackable.Attack);
        var combined = new CombinedEnumerator(
            moveP.GetEnumerator(Square.Zero),
            attackP.GetEnumerator(Square.Zero));

        Assert.That(combined.PositionsAll, Does.ContainKey(new Square { X = 1, Y = 0 }));
        Assert.That(combined.PositionsAll, Does.ContainKey(new Square { X = 0, Y = 1 }));
    }

    [Test]
    public void AttackOnlyCell_IsNotStay()
    {
        var moveP = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var attackP = new Pattern<Square>(new Square { X = 0, Y = 1 }, Attackable.Attack);
        var combined = new CombinedEnumerator(
            moveP.GetEnumerator(Square.Zero),
            attackP.GetEnumerator(Square.Zero));

        // (0,1) came only from attack, so Stayable should be NotStay
        Assert.That(combined.PositionsAll[new Square { X = 0, Y = 1 }],
                    Is.EqualTo(Stayable.NotStay));
    }

    [Test]
    public void ExplicitAttackable_IsPreserved()
    {
        var moveP = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var attackP = new Pattern<Square>(new Square { X = 0, Y = 1 }, Attackable.Attack);
        var combined = new CombinedEnumerator(
            moveP.GetEnumerator(Square.Zero),
            attackP.GetEnumerator(Square.Zero));

        Assert.That(combined.PositionsAttackable, Has.Count.EqualTo(1));
        Assert.That(combined.PositionsAttackable[new Square { X = 0, Y = 1 }],
                    Is.EqualTo(Attackable.Attack));
    }

    [Test]
    public void NonExplicitAttackSource_AllStayablesBecomeAttacks()  // BASELINE CHECK
    {
        var moveP = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var attackArea = new RelativeArea<Square>(
            MultipleAxes.Two, ImmutableHashSet.Create(1), length: 1, isCircle: false, guarantee: true);
        var attackEnum = attackArea.GetEnumerator(Square.Zero);

        var combined = new CombinedEnumerator(moveP.GetEnumerator(Square.Zero), attackEnum);

        // RelativeArea.SupportsExplicitAttackable == false,
        // so CombinedEnumerator promotes attack's PositionsStayables to Attack cells.
        Assert.That(combined.PositionsAttackable.Count,
                    Is.EqualTo(attackEnum.PositionsStayables.Count));
    }

    [Test]
    public void StartPosition_IsAlwaysInPositionsAll()
    {
        var moveP = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var attackP = new Pattern<Square>(new Square { X = 0, Y = 1 }, Attackable.Attack);
        var combined = new CombinedEnumerator(
            moveP.GetEnumerator(Square.Zero),
            attackP.GetEnumerator(Square.Zero));

        Assert.That(combined.PositionsAll, Does.ContainKey(Square.Zero));
    }
}