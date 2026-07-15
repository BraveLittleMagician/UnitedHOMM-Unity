#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

public class GridRenderer : MonoBehaviour
{
    private readonly Dictionary<Vector3Int, GameObject> _cells = new();
    private IEventBus _eventBus = null!;
    private IGlobalGeneratedObjectsHolder _holder = null!;
    private BoxAndAxes? _pendingBuild = null;

    [SerializeField] private GameObject _cellPrefab = null!;
    [SerializeField] private float _cellSize = 1f;

    private void Awake()
    {
        if (_cellPrefab == null) throw new ArgumentNullException(nameof(_cellPrefab));
    }
    private void OnDestroy()
    {
        _eventBus?.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }

    private void CreateCell(Vector3Int coord)
    {
        var pos = GridToWorld(coord);
        var go = Instantiate(_cellPrefab, pos, _cellPrefab.transform.rotation, _holder.Transform);
        go.name = $"Cell_{coord.x}_{coord.y}_{coord.z}";
        _cells[coord] = go;
    }
    private void ClearGrid()
    {
        foreach (var go in _cells.Values) Destroy(go);
        _cells.Clear();
    }
    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        var newConfig = e.NewConfig;
        var axes = (MultipleAxes)newConfig.Axes;
        var box = new AxisAlignedBox(axes, newConfig.FieldSize, 0);
        BuildGridInternal(box, axes);
    }

    [Inject]
    public void Construct(IEventBus eventBus, IGlobalGeneratedObjectsHolder holder)
    {
        _holder = holder ?? throw new ArgumentNullException(nameof(holder));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus)); ;
        _eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
        
        if (_pendingBuild != null)
        {
            BuildGridInternal(_pendingBuild.Box, _pendingBuild.Axes);
            _pendingBuild = null;
        }
    }
    public void BuildGrid(AxisAlignedBox box, MultipleAxes axes)
    {
        if (_eventBus == null)
        {
            _pendingBuild = new(box, axes);
            return;
        }
        BuildGridInternal(box, axes);
    }
    private void BuildGridInternal(AxisAlignedBox box, MultipleAxes axes)
    {
        ClearGrid();

        var activeAxes = axes.GetSeparatedAxes().ToList();
        box.TryGetBounds(Axis.X, out int minX, out int maxX);
        box.TryGetBounds(Axis.Y, out int minY, out int maxY);
        box.TryGetBounds(Axis.Z, out int minZ, out int maxZ);
        box.TryGetBounds(Axis.W, out int minW, out int maxW);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    for (int w = minW; w <= maxW; w++)
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
        var origin = _holder.Transform.position;
        float x = origin.x + coord.x * _cellSize;
        float y = origin.y + coord.y * _cellSize;
        float z = origin.z + coord.z * _cellSize;
        return new Vector3(x, y, z);
    }
    public Vector3 GridToWorld(Square square)
    {
        int x = square.X;
        int y = square.Y;
        int z = square.Z;
        return GridToWorld(new Vector3Int(x, y, z));
    }

    private record BoxAndAxes(AxisAlignedBox Box, MultipleAxes Axes);
}