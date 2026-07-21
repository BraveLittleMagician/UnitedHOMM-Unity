#nullable enable

using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GridBuilder : IGridBuilder
{
    private readonly IObjectResolver _resolver;
    private readonly GameObject _layerPrefab;

    public GridBuilder(IObjectResolver resolver, GameObject layerPrefab)
    {
        _resolver = resolver;
        _layerPrefab = layerPrefab;
    }

    public GameObject BuildGrid(AxisAlignedBox box, Transform parent, float cellSize, float wGroupSpacing)
    {
        var gridRoot = new GameObject("Grid");
        gridRoot.transform.SetParent(parent, false);
        _resolver.InjectGameObject(gridRoot);

        box.TryGetBounds(Axis.X, out int minX, out int maxX);
        box.TryGetBounds(Axis.Y, out int minY, out int maxY);
        box.TryGetBounds(Axis.Z, out int minZ, out int maxZ);
        box.TryGetBounds(Axis.W, out int minW, out int maxW);

        int fieldSizeX = maxX - minX + 1;
        int fieldSizeY = maxY - minY + 1;

        Vector3 startOffset = new(-((maxW - minW) / 2 * (fieldSizeX * cellSize + wGroupSpacing)), 0, 0);

        for (int w = minW; w <= maxW; w++)
        {
            GameObject wGroup = new($"W={w}");
            wGroup.transform.SetParent(gridRoot.transform, false);
            _resolver.InjectGameObject(wGroup);

            Vector3 offset = new((w - minW) * (fieldSizeX * cellSize + wGroupSpacing), 0, 0);
            Vector3 wOffset = startOffset + offset;

            wGroup.transform.localPosition = wOffset;

            for (int z = minZ; z <= maxZ; z++)
            {
                GameObject layer = Object.Instantiate(_layerPrefab, wGroup.transform);
                layer.name = $"W={w}_Z={z}";
                _resolver.InjectGameObject(layer);

                Vector3 posMin = GridToWorldLocal(minX, minY, z, cellSize);
                Vector3 posMax = GridToWorldLocal(maxX, maxY, z, cellSize);
                Vector3 center = (posMin + posMax) / 2f;
                layer.transform.localPosition = center;
                int layerIndex = (w - minW) * (maxZ - minZ + 1) + (z - minZ);
                var sizeSetter = layer.GetComponent<ILayerSizeSetter>();
                sizeSetter?.SetSize(fieldSizeX, fieldSizeY, cellSize, layerIndex);
            }
        }
        return gridRoot;
    }

    public Vector3 GridToWorldLocal(int x, int y, int z, float cellSize) => new (x * cellSize, z * cellSize, y * cellSize);
}