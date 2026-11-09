#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public interface IMovementResolver
{
    bool TryFindPath(PieceModel piece, Vector3Int target, Vector3Int gridSize, [NotNullWhen(true)] out IPath<Square>? path);
    IReadOnlyList<Vector3Int> GetValidMoves(PieceModel piece, Vector3Int gridSize);
    IReadOnlyList<ShapeCell> GetShape(PieceModel piece, Vector3Int gridSize, bool includeOccupied = false);
    ISequenceEnumerator<Square>? CreateEnumerator(PieceModel piece);
}