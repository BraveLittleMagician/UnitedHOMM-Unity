#nullable enable

using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    private readonly Dictionary<Vector2Int, GameObject> _cells = new();

    [SerializeField] private GameObject? _cellPrefab;
    [SerializeField] private int _gridSize = 8;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _cellSpacing = 0.1f;
    [SerializeField] private Vector3 _gridOrigin = Vector3.zero;

    private void Awake()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                var cell = Instantiate(_cellPrefab, transform);
                if (cell == null) continue;
                cell.transform.position = GridToWorld(new Square { X = x, Y = y });
                cell.name = $"Cell_{x}_{y}";
                _cells[new Vector2Int(x, y)] = cell;
            }
        }
    }

    public Vector3 GridToWorld(Square square)
    {
        float x = _gridOrigin.x + square.X * (_cellSize + _cellSpacing);
        float y = _gridOrigin.y + square.Y * (_cellSize + _cellSpacing);
        return new Vector3(x, y, 0);
    }
}