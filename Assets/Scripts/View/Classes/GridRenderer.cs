#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    private readonly Dictionary<Vector3Int, GameObject> _cells = new();

    [SerializeField] private GameObject _cellPrefab = null!;
    [SerializeField] private int _gridSize = 8;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _cellSpacing = 0f;

    private void Awake()
    {
        if (_cellPrefab == null) throw new NullReferenceException(nameof(_cellPrefab));
    }

    public void BuildGrid(BoardConfig config, AxisAlignedBox box)
    {
        ClearGrid();

        var axes = config.Is3D ? MultipleAxes.Three : MultipleAxes.Two;
        int fieldSize = config.FieldSize;
        int frameThickness = config.FrameThickness;

        var activeAxes = axes.GetSeparatedAxes().ToList();

        int minField = frameThickness;
        int maxField = frameThickness + fieldSize - 1;
        int totalSize = fieldSize + 2 * frameThickness;

        for (int x = 0; x < totalSize; x++)
        {
            for (int y = 0; y < (activeAxes.Contains(Axis.Y) ? totalSize : 1); y++)
            {
                for (int z = 0; z < (activeAxes.Contains(Axis.Z) ? totalSize : 1); z++)
                {
                    if (!activeAxes.Contains(Axis.X) && x != 0) continue;
                    if (!activeAxes.Contains(Axis.Y) && y != 0) continue;
                    if (!activeAxes.Contains(Axis.Z) && z != 0) continue;

                    var coord = new Vector3Int(x, z, y);
                    var square = new Square(new Dictionary<Axis, int>
                    {
                        [Axis.X] = x,
                        [Axis.Y] = y,
                        [Axis.Z] = z,
                        [Axis.W] = 0
                    });
                    if (!box.Contains(square)) continue;

                    bool isField = (x >= frameThickness && x < frameThickness + fieldSize) &&
                                   (y >= frameThickness && y < frameThickness + fieldSize) &&
                                   (z >= frameThickness && z < frameThickness + fieldSize);

                    CreateCell(coord);
                }
            }
        }
    }

    private void CreateCell(Vector3Int coord)
    {
        var pos = GridToWorld(coord);
        var go = Instantiate(_cellPrefab, pos, _cellPrefab.transform.rotation, GlobalGeneratedObjectsHolder.Instance.transform);
        go.name = $"Cell_{coord.x}_{coord.y}_{coord.z}";
        _cells[coord] = go;
    }

    private void ClearGrid()
    {
        foreach (var go in _cells.Values) Destroy(go);
        _cells.Clear();
    }

    public Vector3 GridToWorld(Vector3Int coord)
    {
        float x = GlobalGeneratedObjectsHolder.Instance.transform.position.x + coord.x * (_cellSize + _cellSpacing);
        float y = GlobalGeneratedObjectsHolder.Instance.transform.position.y + coord.y * (_cellSize + _cellSpacing);
        float z = GlobalGeneratedObjectsHolder.Instance.transform.position.z + coord.z * (_cellSize + _cellSpacing);
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