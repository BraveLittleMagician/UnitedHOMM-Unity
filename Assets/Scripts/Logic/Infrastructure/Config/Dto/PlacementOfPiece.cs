#nullable enable

using UnityEngine;

public record PlacementOfPiece(Vector3Int Position, string Name, IdOfPlayer Player)
{
    public FacingDirection Facing { get; init; } = FacingDirection.PlusY;
}