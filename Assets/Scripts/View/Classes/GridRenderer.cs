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
    private GameObject? _gridRoot = null;
    private AxisAlignedBox? _pendingBox = null;

    [SerializeField] private GameObject _layerPrefab = null!;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _wGroupSpacing = 1.5f;

    private void Awake()
    {
        if (_layerPrefab == null) throw new ArgumentNullException(nameof(_layerPrefab));
    }
    private void OnDestroy()
    {
        _eventBus?.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged); 
        ClearGrid();
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
        var box = new AxisAlignedBox(axes, newConfig.FieldSize, newConfig.WUp, newConfig.WDown);
        BuildGridInternal(box);
    }
    private void BuildGridInternal(AxisAlignedBox box)
    {
        ClearGrid();

        box.TryGetBounds(Axis.X, out int minX, out int maxX);
        box.TryGetBounds(Axis.Y, out int minY, out int maxY);
        box.TryGetBounds(Axis.Z, out int minZ, out int maxZ);
        box.TryGetBounds(Axis.W, out int minW, out int maxW);

        int fieldSizeX = maxX - minX + 1;
        int fieldSizeY = maxY - minY + 1;

        _gridRoot = new GameObject("Grid");
        _gridRoot.transform.SetParent(_holder.Transform, false);
        _resolver.InjectGameObject(_gridRoot);

        Vector3 startOffset = new(-((maxW - minW) / 2 * (fieldSizeX * _cellSize + _wGroupSpacing)), 0, 0);

        for (int w = minW; w <= maxW; w++)
        {
            GameObject wGroup = new ($"W={w}");
            wGroup.transform.SetParent(_gridRoot.transform, false);
            _resolver.InjectGameObject(wGroup);

            Vector3 offset = new ((w - minW) * (fieldSizeX * _cellSize + _wGroupSpacing), 0, 0 );
            Vector3 wOffset = startOffset + offset;

            wGroup.transform.localPosition = wOffset;

            for (int z = minZ; z <= maxZ; z++)
            {
                GameObject layer = Instantiate(_layerPrefab, wGroup.transform);
                layer.name = $"W={w}_Z={z}";
                _resolver.InjectGameObject(layer);

                Vector3 posMin = GridToWorldLocal(minX, minY, z);
                Vector3 posMax = GridToWorldLocal(maxX, maxY, z);
                Vector3 center = (posMin + posMax) / 2f;
                layer.transform.localPosition = center;
                int layerIndex = (w - minW) * (maxZ - minZ + 1) + (z - minZ);
                var sizeSetter = layer.GetComponent<ILayerSizeSetter>();
                sizeSetter?.SetSize(fieldSizeX, fieldSizeY, _cellSize, layerIndex);
            }
        }
    }
    private Vector3 GridToWorldLocal(int x, int y, int z)
    {
        float wx = x * _cellSize;
        float wy = z * _cellSize;
        float wz = y * _cellSize;
        return new Vector3(wx, wy, wz);
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