#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    private readonly Dictionary<Vector3Int, GameObject> _cells = new();

    [SerializeField] private GameObject _terrainPrefab = null!;
    [SerializeField] private float _cellSize = 1f;

    private void Awake()
    {
        if (_terrainPrefab == null) throw new NullReferenceException(nameof(_terrainPrefab));
    }
    private void CreateCell(Vector3Int coord)
    {
        var pos = GridToWorld(coord);
        var go = Instantiate(_terrainPrefab, pos, _terrainPrefab.transform.rotation, GlobalGeneratedObjectsHolder.Instance.transform);
        go.name = $"Cell_{coord.x}_{coord.y}_{coord.z}";
        _cells[coord] = go;
    }
    private void ClearGrid()
    {
        foreach (var go in _cells.Values) Destroy(go);
        _cells.Clear();
    }

    public void BuildGrid(AxisAlignedBox box, MultipleAxes axes)
    {
        ClearGrid();

        var activeAxes = axes.GetSeparatedAxes().ToList();
        box.TryGetBounds(Axis.X, out int minX, out int maxX);
        box.TryGetBounds(Axis.Y, out int minY, out int maxY);
        box.TryGetBounds(Axis.Z, out int minZ, out int maxZ);
        box.TryGetBounds(Axis.W, out int minW, out int maxW);
        maxX = Math.Max(maxX, 1);
        maxY = Math.Max(maxY, 1);
        maxZ = Math.Max(maxZ, 1);
        maxW = Math.Max(maxW, 1);

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    for (int w = minW; w < maxW; w++)
                    {
                        if (!activeAxes.Contains(Axis.X) && x != 0) continue;
                        if (!activeAxes.Contains(Axis.Y) && y != 0) continue;
                        if (!activeAxes.Contains(Axis.Z) && z != 0) continue;
                        if (!activeAxes.Contains(Axis.W) && w != 0) continue;

                        var coord = new Vector3Int(x, z, y);
                        var square = new Square(new Dictionary<Axis, int>
                        {
                            [Axis.X] = x,
                            [Axis.Y] = y,
                            [Axis.Z] = z,
                            [Axis.W] = w
                        });
                        if (!box.Contains(square)) continue;

                        CreateCell(coord);
                    }
                }
            }
        }
    }
    public Vector3 GridToWorld(Vector3Int coord)
    {
        float x = GlobalGeneratedObjectsHolder.Instance.transform.position.x + coord.x * (_cellSize);
        float y = GlobalGeneratedObjectsHolder.Instance.transform.position.y + coord.y * (_cellSize);
        float z = GlobalGeneratedObjectsHolder.Instance.transform.position.z + coord.z * (_cellSize);
        return new Vector3(x, y, z);
    }
    public Vector3 GridToWorld(Square square)
    {
        int x = square.X;
        int y = square.Y;
        int z = square.Z;
        return GridToWorld(new Vector3Int(x, y, z));
    }
}