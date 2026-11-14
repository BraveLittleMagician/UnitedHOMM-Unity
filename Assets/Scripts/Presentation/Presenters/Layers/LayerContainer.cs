#nullable enable
using UnityEngine;
using UnityEngine.Rendering;

public class LayerContainer : MonoBehaviour
{
    public int Axis { get; private set; }
    public int LayerIndex { get; private set; }
    public int PieceCount { get; private set; }
    public float Step { get; private set; }
    public Square GridSize { get; private set; }

    public Vector3 TargetLocalPosition { get; private set; }

    public Vector3 TargetWorldPosition
    {
        get
        {
            var parent = transform.parent;
            return parent != null
                ? parent.TransformPoint(TargetLocalPosition)
                : TargetLocalPosition;
        }
    }

    public Transform? TargetTwin { get; private set; }
    private Transform? _visual;

    public void Initialize(
        int axis,
        int layerIndex,
        Square gridSize,
        float step,
        Vector3 baseLocalPosition,
        Material? visualMaterial)
    {
        Axis = axis;
        LayerIndex = layerIndex;
        GridSize = gridSize;
        Step = step;
        if (visualMaterial != null)
            CreateVisual(GridAxes.ThinnedWorldSize(axis, gridSize, step), visualMaterial);
        MoveTo(baseLocalPosition);
    }

    private void CreateVisual(Vector3 size, Material material)
    {
        var visualGo = new GameObject("Visual");
        visualGo.transform.SetParent(transform, false);
        visualGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        var mf = visualGo.AddComponent<MeshFilter>();
        mf.sharedMesh = BoxMeshCache.Get(size);
        var rend = visualGo.AddComponent<MeshRenderer>();
        rend.sharedMaterial = material;
        rend.shadowCastingMode = ShadowCastingMode.Off;
        rend.receiveShadows = false;
        _visual = visualGo.transform;
    }

    public void ResetPieceCount() => PieceCount = 0;

    public void AttachTargetTwin(Transform twin)
    {
        TargetTwin = twin;
        TargetTwin.localPosition = TargetLocalPosition;
    }

    public void UpdateSize(float step)
    {
        Step = step;
        Vector3 size = GridAxes.ThinnedWorldSize(Axis, GridSize, step);
        if (_visual != null && _visual.TryGetComponent<MeshFilter>(out var mf))
            mf.sharedMesh = BoxMeshCache.Get(size);
    }

    public void UpdateGridSize(Square gridSize, float step)
    {
        GridSize = gridSize;
        Step = step;
        Vector3 size = GridAxes.ThinnedWorldSize(Axis, gridSize, step);
        if (_visual != null && _visual.TryGetComponent<MeshFilter>(out var mf))
            mf.sharedMesh = BoxMeshCache.Get(size);
    }

    public void AddPiece(GameObject piece)
    {
        piece.transform.SetParent(transform, false);
        PieceCount++;
    }

    public void MoveTo(Vector3 position)
    {
        TargetLocalPosition = position;
        transform.localPosition = position;
        if (TargetTwin != null) TargetTwin.localPosition = position;
    }

    public void SetVisibilitySelf(int targetLayer)
    {
        gameObject.layer = targetLayer;
        if (_visual != null) _visual.gameObject.SetLayerRecursively(targetLayer);
        if (TargetTwin != null) TargetTwin.gameObject.layer = targetLayer;
    }

    public void SetVisibilityRecursively(int targetLayer)
    {
        gameObject.SetLayerRecursively(targetLayer);
        if (TargetTwin != null) TargetTwin.gameObject.SetLayerRecursively(targetLayer);
    }
}