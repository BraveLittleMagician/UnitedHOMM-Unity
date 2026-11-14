#nullable enable

using System;

public static partial class FacingRotation
{
    public static Square Rotate(Square local, FacingDirection facing) => facing switch
    {
        FacingDirection.PlusY => local,
        FacingDirection.PlusX => new Square { X = local.Y, Y = -local.X, Z = local.Z, W = local.W },
        FacingDirection.MinusY => new Square { X = -local.X, Y = -local.Y, Z = local.Z, W = local.W },
        FacingDirection.MinusX => new Square { X = -local.Y, Y = local.X, Z = local.Z, W = local.W },
        _ => throw new InvalidOperationException($"Facing '{facing}' is not valid for shape rotation."),
    };

    public static Square Unrotate(Square world, FacingDirection facing) => facing switch
    {
        FacingDirection.PlusY => world,
        FacingDirection.PlusX => new Square { X = -world.Y, Y = world.X, Z = world.Z, W = world.W },
        FacingDirection.MinusY => new Square { X = -world.X, Y = -world.Y, Z = world.Z, W = world.W },
        FacingDirection.MinusX => new Square { X = world.Y, Y = -world.X, Z = world.Z, W = world.W },
        _ => throw new InvalidOperationException($"Facing '{facing}' is not valid for shape rotation."),
    };
}