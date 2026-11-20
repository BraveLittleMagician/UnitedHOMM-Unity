#nullable enable

using NUnit.Framework;

public sealed class FacingTests
{
    private static readonly Square[] SampleLocals =
    {
            new Square { X = 1, Y = 0 },
            new Square { X = 0, Y = 1 },
            new Square { X = -1, Y = 1 },
            new Square { X = 3, Y = -2 },
        };

    [Test]
    public void RotateUnrotate_RoundTrip_AllDirections()
    {
        var facings = new[]
        {
                FacingDirection.PlusY, FacingDirection.PlusX,
                FacingDirection.MinusY, FacingDirection.MinusX,
            };
        foreach (var facing in facings)
            foreach (var local in SampleLocals)
            {
                var world = FacingRotation.Rotate(local, facing);
                var back = FacingRotation.Unrotate(world, facing);
                Assert.That(back, Is.EqualTo(local),
                    $"Round-trip failed for {facing}, local=({local.X},{local.Y})");
            }
    }

    [Test]
    public void PlusY_IsIdentity()
    {
        foreach (var local in SampleLocals)
            Assert.That(FacingRotation.Rotate(local, FacingDirection.PlusY),
                        Is.EqualTo(local));
    }

    [Test]
    public void PlusX_RotatesForwardToRight()  // BASELINE CHECK
    {
        // Local "forward" is +Y. After PlusX rotation, forward should become +X.
        var forward = new Square { X = 0, Y = 1 };
        var rotated = FacingRotation.Rotate(forward, FacingDirection.PlusX);

        Assert.That(rotated, Is.EqualTo(new Square { X = 1, Y = 0 }));
    }

    [Test]
    public void FacingEnumerator_PlusY_PreservesInnerPositions()
    {
        var pattern = new Pattern<Square>(new Square { X = 1, Y = 0 });
        var inner = pattern.GetEnumerator(Square.Zero);
        var facing = new FacingEnumerator(inner, FacingDirection.PlusY, Square.Zero);

        Assert.That(facing.PositionsStayables,
                    Is.EquivalentTo(inner.PositionsStayables));
    }

    [Test]
    public void FacingEnumerator_PlusX_RotatesForwardToRight()  // BASELINE CHECK
    {
        var forward = new Pattern<Square>(new Square { X = 0, Y = 1 });
        var inner = forward.GetEnumerator(Square.Zero);
        var facing = new FacingEnumerator(inner, FacingDirection.PlusX, Square.Zero);

        Assert.That(facing.PositionsStayables, Has.Count.EqualTo(1));
        Assert.That(facing.PositionsStayables[0], Is.EqualTo(new Square { X = 1, Y = 0 }));
    }

    [Test]
    public void FacingEnumerator_CanMoveTo_RotatesQueryBack()
    {
        var forward = new Pattern<Square>(new Square { X = 0, Y = 1 });
        var inner = forward.GetEnumerator(Square.Zero);
        var facing = new FacingEnumerator(inner, FacingDirection.PlusX, Square.Zero);

        // World query: can I move to (1,0)? Yes (rotated local (0,1) -> world (1,0)).
        Assert.That(facing.CanMoveTo(new Square { X = 1, Y = 0 }), Is.True);
        // World query: can I move to (0,1)? No (local (0,1) unrotates to (1,0)... actually wait).
        // Unrotate(world=(0,1), PlusX) = (X=world.Y, Y=-world.X) = (1, 0).
        // Local (1,0) is not in inner's stayables (inner has (0,1) only).
        Assert.That(facing.CanMoveTo(new Square { X = 0, Y = 1 }), Is.False);
    }
}