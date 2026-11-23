#nullable enable

using System.Collections.Immutable;
using NUnit.Framework;

public sealed class InvariantsTests
{
    private static System.Collections.Generic.List<ISequenceEnumerator<Square>> AllEnumerators()
    {
        var list = new System.Collections.Generic.List<ISequenceEnumerator<Square>>();

        // Pattern
        list.Add(new Pattern<Square>(new Square { X = 1, Y = 0 }).GetEnumerator(Square.Zero));
        list.Add(new Pattern<Square>(new Square { X = 1, Y = 0 }).GetEnumerator(Square.Zero));

        // King via dictionary
        var kingDict = ImmutableDictionary.CreateBuilder<Square, Stayable>();
        kingDict[Square.Zero] = Stayable.NotStay;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                kingDict[new Square { X = dx, Y = dy }] = Stayable.Stay;
            }
        list.Add(new Pattern<Square>(kingDict.ToImmutable()).GetEnumerator(Square.Zero));

        // RelativeArea
        list.Add(new RelativeArea<Square>(
            MultipleAxes.Two, ImmutableHashSet.Create(1), length: 1, isCircle: false)
            .GetEnumerator(Square.Zero));

        // Combined
        list.Add(new CombinedEnumerator(
            new Pattern<Square>(new Square { X = 1, Y = 0 }).GetEnumerator(Square.Zero),
            new Pattern<Square>(new Square { X = 0, Y = 1 }).GetEnumerator(Square.Zero)));

        return list;
    }

    [Test]
    public void Attackable_IsSubsetOfPositionsAll()
    {
        foreach (var e in AllEnumerators())
            foreach (var attackCell in e.PositionsAttackable.Keys)
                Assert.That(e.PositionsAll, Does.ContainKey(attackCell),
                    $"Cell ({attackCell.X},{attackCell.Y}) is attackable but missing from PositionsAll");
    }

    [Test]
    public void Stayables_IsSubsetOfPositionsAll()
    {
        foreach (var e in AllEnumerators())
            foreach (var stayCell in e.PositionsStayables)
                Assert.That(e.PositionsAll, Does.ContainKey(stayCell),
                    $"Cell ({stayCell.X},{stayCell.Y}) is stayable but missing from PositionsAll");
    }

    [Test]
    public void Stayables_MatchStayFlagInPositionsAll()
    {
        foreach (var e in AllEnumerators())
        {
            var expected = new System.Collections.Generic.HashSet<Square>();
            foreach (var kv in e.PositionsAll)
                if (kv.Value == Stayable.Stay) expected.Add(kv.Key);

            Assert.That(new System.Collections.Generic.HashSet<Square>(e.PositionsStayables),
                        Is.EquivalentTo(expected),
                        "PositionsStayables must equal { cell | PositionsAll[cell] == Stayable.Stay }");
        }
    }

    [Test]
    public void StartPosition_IsInPositionsAll()
    {
        foreach (var e in AllEnumerators())
            Assert.That(e.PositionsAll, Does.ContainKey(e.StartPosition));
    }
}