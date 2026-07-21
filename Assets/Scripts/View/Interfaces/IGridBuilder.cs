#nullable enable

using UnityEngine;

public interface IGridBuilder
{
    GameObject BuildGrid(AxisAlignedBox box, Transform parent, float cellSize, float wGroupSpacing);
    Vector3 GridToWorldLocal(int x, int y, int z, float cellSize);
}