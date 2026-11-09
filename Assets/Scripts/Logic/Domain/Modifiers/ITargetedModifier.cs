#nullable enable
using System.Collections.Generic;
using UnityEngine;

public interface ITargetedModifier : IActiveModifier
{
    IReadOnlyList<Vector3Int> GetTargetCells(PieceModel owner, Vector3Int gridSize);
    void Activate(ModifierActivationContext context, Vector3Int targetCell);
}