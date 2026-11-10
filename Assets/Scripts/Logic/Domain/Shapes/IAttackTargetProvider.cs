#nullable enable

using System.Collections.Generic;
using UnityEngine;

public interface IAttackTargetProvider
{
    bool TryFindAttackPath(PieceModel attacker, Vector3Int target, Vector3Int gridSize, out IPath<Square>? path);
    bool IsAttackTarget(PieceModel attacker, Vector3Int cell, Vector3Int gridSize);
    IReadOnlyList<Vector3Int> GetAttackTargets(PieceModel attacker, Vector3Int gridSize);
}