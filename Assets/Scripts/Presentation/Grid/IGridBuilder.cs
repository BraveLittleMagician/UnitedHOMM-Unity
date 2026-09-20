#nullable enable

using System.Collections.Generic;
using UnityEngine;

public interface IGridBuilder
{
    GameObject BuildGrid(IReadOnlyDictionary<Axis, (int Min, int Max)> box, Transform parent, float cellSize, float wGroupSpacing);
    Vector3 GridToWorldLocal(int x, int y, int z, float cellSize);
}