#nullable enable

using System.Collections.Generic;
using UnityEngine;

public class GridBuilder : IGridBuilder
{
    private readonly GameObject _layerPrefab;

    public GridBuilder(GameObject layerPrefab)
    {
        _layerPrefab = layerPrefab;
    }

    public GameObject BuildGrid(IReadOnlyDictionary<Axis, (int Min, int Max)> box, Transform parent, float cellSize, float wGroupSpacing)
    {
        var gridRoot = new GameObject("Grid");
        gridRoot.transform.SetParent(parent, false);


        box.TryGetValue(Axis.X, out var xPair);
        box.TryGetValue(Axis.Y, out var yPair);
        box.TryGetValue(Axis.Z, out var zPair);
        box.TryGetValue(Axis.W, out var wPair);

        int fieldSizeX = xPair.Max - xPair.Min + 1;
        int fieldSizeY = yPair.Max - yPair.Min + 1;

        Vector3 startOffset = new(-((wPair.Max - wPair.Min) / 2 * (fieldSizeX * cellSize + wGroupSpacing)), 0, 0);

        for (int w = wPair.Min; w <= wPair.Max; w++)
        {
            GameObject wGroup = new($"W={w}");
            wGroup.transform.SetParent(gridRoot.transform, false);

            Vector3 offset = new((w - wPair.Min) * (fieldSizeX * cellSize + wGroupSpacing), 0, 0);
            Vector3 wOffset = startOffset + offset;

            wGroup.transform.localPosition = wOffset;

            for (int z = zPair.Min; z <= zPair.Max; z++)
            {
                GameObject layer = Object.Instantiate(_layerPrefab, wGroup.transform);
                layer.name = $"W={w}_Z={z}";

                Vector3 posMin = GridToWorldLocal(xPair.Min, yPair.Min, z, cellSize);
                Vector3 posMax = GridToWorldLocal(xPair.Max, yPair.Max, z, cellSize);
                Vector3 center = (posMin + posMax) / 2f;
                layer.transform.localPosition = center;
                int layerIndex = (w - wPair.Min) * (zPair.Max - zPair.Min + 1) + (z - zPair.Min);
                var sizeSetter = layer.GetComponent<ILayerSizeSetter>();
                sizeSetter?.SetSize(fieldSizeX, fieldSizeY, cellSize, layerIndex);
            }
        }
        return gridRoot;
    }
    public Vector3 GridToWorldLocal(int x, int y, int z, float cellSize) => new (x * cellSize, z * cellSize, y * cellSize);
}