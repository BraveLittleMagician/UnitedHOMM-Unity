#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GridRenderer : MonoBehaviour
{
    private IEventBus _eventBus = null!;
    private IGlobalGeneratedSurroundingsHolder _holder = null!;
    private IObjectResolver _resolver = null!;
    private AxisAlignedBox? _pendingBox = null;
    private GameObject? _gridRoot = null;

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

    private void ClearGrid()
    {
        if (_gridRoot != null)
        {
            Destroy(_gridRoot);
            _gridRoot = null;
        }
    }
    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        var newConfig = e.NewConfig;
        var axes = (MultipleAxes)newConfig.Axes;
        var box = new AxisAlignedBox(axes, newConfig.FieldSize, 0);
        BuildGridInternal(box);
    }
    private void BuildGridInternal(AxisAlignedBox box)
    {
        ClearGrid();

        var orderedAxes = new Axis[] { Axis.W, Axis.Z, Axis.Y, Axis.X }.ToArray();

        box.TryGetBounds(Axis.X, out int minX, out int maxX);
        box.TryGetBounds(Axis.Y, out int minY, out int maxY);
        box.TryGetBounds(Axis.Z, out int minZ, out int maxZ);
        box.TryGetBounds(Axis.W, out int minW, out int maxW);

        _gridRoot = new GameObject("Grid");
        _gridRoot.transform.SetParent(_holder.Transform, false);
        _resolver.InjectGameObject(_gridRoot);

        var ranges = new Dictionary<Axis, (int Min, int Max)>
        {
            [Axis.X] = (minX, maxX),
            [Axis.Y] = (minY, maxY),
            [Axis.Z] = (minZ, maxZ),
            [Axis.W] = (minW, maxW)
        };

        BuildHierarchy(_gridRoot.transform, orderedAxes, 0, new Dictionary<Axis, int>(), ranges);
    }
    private void BuildHierarchy(Transform parent, Axis[] axes, int axisIndex, Dictionary<Axis, int> currentCoords, Dictionary<Axis, (int Min, int Max)> ranges)
    {
        if (axisIndex >= axes.Length) return;

        Axis currentAxis = axes[axisIndex];
        var (min, max) = ranges[currentAxis];

        if (currentAxis == Axis.X)
        {
            for (int x = min; x <= max; x++)
            {
                var cellCoords = new Dictionary<Axis, int>(currentCoords) { [Axis.X] = x };
                var pos = GridToWorld(cellCoords);
                var go = Instantiate(_cellPrefab, pos, _cellPrefab.transform.rotation, parent);
                go.name = $"{currentAxis}={x} (cell)";
                _resolver.InjectGameObject(go);

            }
            return;
        }
        else
        {
            for (int value = min; value <= max; value++)
            {
                var group = new GameObject($"{currentAxis}={value}");
                group.transform.SetParent(parent, false);
                _resolver.InjectGameObject(group);
                var newCoords = new Dictionary<Axis, int>(currentCoords) { [currentAxis] = value };
                BuildHierarchy(group.transform, axes, axisIndex + 1, newCoords, ranges);
            }
        }
    }
    private Vector3 GridToWorld(Dictionary<Axis, int> coords)
    {
        var origin = _holder.Transform.position;
        float x = origin.x + coords[Axis.X] * _cellSize;
        float y = origin.y + coords[Axis.Y] * _cellSize;
        float z = origin.z + coords[Axis.Z] * _cellSize;
        return new Vector3(x, z, y);
    }

    [Inject]
    public void Construct(IEventBus eventBus, IGlobalGeneratedSurroundingsHolder holder, IObjectResolver resolver)
    {
        _holder = holder ?? throw new ArgumentNullException(nameof(holder));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);

        if (_pendingBox != null)
        {
            BuildGridInternal(_pendingBox);
            _pendingBox = null;
        }
    }
    public void BuildGrid(AxisAlignedBox box)
    {
        if (_eventBus == null)
        {
            _pendingBox = box;
            return;
        }
        BuildGridInternal(box);
    }

    public Vector3 GridToWorld(Square square) => GridToWorld(new Dictionary<Axis, int>
    {
        [Axis.X] = square.X,
        [Axis.Y] = square.Y,
        [Axis.Z] = square.Z,
        [Axis.W] = 0
    });
}