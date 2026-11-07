#nullable enable

using System;
using UnityEngine;

public static class FacingDirectionExtensions
{
    public static Vector3Int ToUnitVector(this FacingDirection facing) => facing switch
    {
        FacingDirection.PlusX => new Vector3Int(1, 0, 0),
        FacingDirection.MinusX => new Vector3Int(-1, 0, 0),
        FacingDirection.PlusY => new Vector3Int(0, 1, 0),
        FacingDirection.MinusY => new Vector3Int(0, -1, 0),
        FacingDirection.PlusZ => new Vector3Int(0, 0, 1),
        FacingDirection.MinusZ => new Vector3Int(0, 0, -1),
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, null),
    };
}