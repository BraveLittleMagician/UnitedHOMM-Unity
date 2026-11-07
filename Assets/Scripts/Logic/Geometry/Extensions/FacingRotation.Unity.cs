#nullable enable

using UnityEngine;

public static partial class FacingRotation 
{
    public static Vector3Int Rotate(Vector3Int local, FacingDirection facing)
    {
        var s = Rotate(new Square { X = local.x, Y = local.y, Z = local.z }, facing);
        return new Vector3Int(s.X, s.Y, s.Z);
    }

    public static Vector3Int Unrotate(Vector3Int world, FacingDirection facing)
    {
        var s = Unrotate(new Square { X = world.x, Y = world.y, Z = world.z }, facing);
        return new Vector3Int(s.X, s.Y, s.Z);
    }
}