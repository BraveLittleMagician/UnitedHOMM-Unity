#nullable enable

using System.Collections.Immutable;
using NUnit.Framework;

public sealed class RelativeAreaTests
{
    [Test]
    public void LargerRadius_HasMorePositions()
    {
        var small = new RelativeArea<Square>(
            MultipleAxes.Two, ImmutableHashSet.Create(1), length: 1, isCircle: false, guarantee: true);
        var large = new RelativeArea<Square>(
            MultipleAxes.Two, ImmutableHashSet.Create(1, 2, 3), length: 3, isCircle: false, guarantee: true);

        var smallCount = small.GetEnumerator(Square.Zero).PositionsAll.Count;
        var largeCount = large.GetEnumerator(Square.Zero).PositionsAll.Count;

        Assert.That(largeCount, Is.GreaterThan(smallCount),
            "radius 3 should cover more cells than radius 1");
    }

    [Test]
    public void StartPosition_IsInPositionsAll()  // BASELINE CHECK
    {
        var area = new RelativeArea<Square>(
            MultipleAxes.Two, ImmutableHashSet.Create(1), length: 1, isCircle: false, guarantee: true);
        var e = area.GetEnumerator(Square.Zero);

        Assert.That(e.PositionsAll, Does.ContainKey(Square.Zero));
    }

    [Test]
    public void SupportsExplicitAttackable_IsFalse()  // BASELINE CHECK
    {
        var area = new RelativeArea<Square>(
            MultipleAxes.Two, ImmutableHashSet.Create(1), length: 1, isCircle: false, guarantee: true);
        var e = area.GetEnumerator(Square.Zero);

        Assert.That(e.SupportsExplicitAttackable, Is.False,
            "RelativeArea does not track explicit attack cells; CombinedEnumerator relies on this");
    }

    [Test]
    public void SquareVsCircle_ShapesDiffer()  // BASELINE CHECK
    {
        var square = new RelativeArea<Square>(
            MultipleAxes.Two, ImmutableHashSet.Create(1, 2), length: 2, isCircle: false, guarantee: true);
        var circle = new RelativeArea<Square>(
            MultipleAxes.Two, ImmutableHashSet.Create(1, 2), length: 2, isCircle: true, guarantee: true);

        var sqCount = square.GetEnumerator(Square.Zero).PositionsAll.Count;
        var circleCount = circle.GetEnumerator(Square.Zero).PositionsAll.Count;

        // If this fails, it means "isCircle" semantics differ from my assumption.
        // Inspect FieldExtensions.AdjacentPositions vs AdjacentPositionsCircle.
        Assert.That(sqCount, Is.Not.EqualTo(circleCount));
    }
}