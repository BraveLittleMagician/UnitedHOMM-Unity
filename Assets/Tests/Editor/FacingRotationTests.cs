#nullable enable

using NUnit.Framework;

public class FacingRotationTests
{
    private static readonly FacingDirection[] _validFacings =
    {
        FacingDirection.PlusX,
        FacingDirection.MinusX,
        FacingDirection.PlusY,
        FacingDirection.MinusY,
    };

    private static readonly Square[] _samples =
    {
        new () { X = 0, Y = 1, Z = 0 },        // forward
        new () { X = 3, Y = 4, Z = 1 },        // общий случай
        new () { X = -2, Y = 5, Z = 0 },       // отрицательная X
        new () { X = 0, Y = 0, Z = 2 },        // только Z
        new () { X = 1, Y = 0, Z = 0 },        // только X
        Square.Zero,                                 // origin
    };

    [Test]
    public void Rotate_ThenUnrotate_IsIdentity_ForAllValidFacings()
    {
        foreach (var facing in _validFacings)
            foreach (var sample in _samples)
            {
                var rotated = FacingRotation.Rotate(sample, facing);
                var back = FacingRotation.Unrotate(rotated, facing);
                Assert.AreEqual(sample, back,
                    $"Rotate→Unrotate failed for facing={facing}, square={sample}");
            }
    }

    [Test]
    public void Unrotate_ThenRotate_IsIdentity_ForAllValidFacings()
    {
        foreach (var facing in _validFacings)
            foreach (var sample in _samples)
            {
                var unrotated = FacingRotation.Unrotate(sample, facing);
                var back = FacingRotation.Rotate(unrotated, facing);
                Assert.AreEqual(sample, back,
                    $"Unrotate→Rotate failed for facing={facing}, square={sample}");
            }
    }

    [Test]
    public void Rotate_PlusY_IsIdentity()
    {
        foreach (var sample in _samples)
            Assert.AreEqual(sample, FacingRotation.Rotate(sample, FacingDirection.PlusY));
    }

    [Test]
    public void Rotate_PlusX_TurnsForward_IntoRight()
    {
        // forward (0,1,0) под PlusX должен стать (1,0,0)
        var forward = new Square { X = 0, Y = 1, Z = 0 };
        var rotated = FacingRotation.Rotate(forward, FacingDirection.PlusX);
        Assert.AreEqual(new Square { X = 1, Y = 0, Z = 0 }, rotated);
    }

    [Test]
    public void Rotate_MinusY_TurnsForward_IntoBackward()
    {
        // forward (0,1,0) под MinusY должен стать (0,-1,0)
        var forward = new Square { X = 0, Y = 1, Z = 0 };
        var rotated = FacingRotation.Rotate(forward, FacingDirection.MinusY);
        Assert.AreEqual(new Square { X = 0, Y = -1, Z = 0 }, rotated);
    }

    [Test]
    public void Rotate_MinusX_TurnsForward_IntoLeft()
    {
        // forward (0,1,0) под MinusX должен стать (-1,0,0)
        var forward = new Square { X = 0, Y = 1, Z = 0 };
        var rotated = FacingRotation.Rotate(forward, FacingDirection.MinusX);
        Assert.AreEqual(new Square { X = -1, Y = 0, Z = 0 }, rotated);
    }

    [Test]
    public void Rotate_KeepsZAndW_Unchanged()
    {
        var sample = new Square { X = 2, Y = 3, Z = 7, W = -1 };
        foreach (var facing in _validFacings)
        {
            var rotated = FacingRotation.Rotate(sample, facing);
            Assert.AreEqual(7, rotated.Z, $"Z changed for facing={facing}");
            Assert.AreEqual(-1, rotated.W, $"W changed for facing={facing}");
        }
    }

    [Test]
    public void Rotate_PlusZ_Throws()
    {
        Assert.Throws<System.InvalidOperationException>(() =>
            FacingRotation.Rotate(Square.Zero, FacingDirection.PlusZ));
    }

    [Test]
    public void Rotate_MinusZ_Throws()
    {
        Assert.Throws<System.InvalidOperationException>(() =>
            FacingRotation.Rotate(Square.Zero, FacingDirection.MinusZ));
    }
}