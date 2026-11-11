#nullable enable

using System;
using UnityEngine;

public sealed class LayerCameraWork : MonoBehaviour, IRunnable, IDisposable
{
    [SerializeField] private Camera _layerCamera = null!;
    [SerializeField] private float _padding = 1.15f;
    [SerializeField] private string _activeLayerName = "ActiveLayer";
    [SerializeField] private string _decorLayerName = "LayerViewDecor";

    private LoaderOfBoard _boardLoader = null!;
    private LayerPresenter _layerPresenter = null!;
    private ViewState _viewState = null!;
    private bool _disposed;

    public void Initialize(
        LayerPresenter layerPresenter,
        LoaderOfBoard boardLoader,
        ViewState viewState,
        Camera layerCamera)
    {
        _layerPresenter = layerPresenter ?? throw new ArgumentNullException(nameof(layerPresenter));
        _boardLoader = boardLoader ?? throw new ArgumentNullException(nameof(boardLoader));
        _viewState = viewState ?? throw new ArgumentNullException(nameof(viewState));
        _layerCamera = layerCamera != null ? layerCamera : throw new ArgumentNullException(nameof(layerCamera));
    }

    public void Run()
    {
        if (_layerCamera == null) throw new InvalidOperationException("LayerCameraWork not initialized.");
        _layerCamera.rect = new Rect(ViewportLayout.LeftCameraWidthFraction, 0f, ViewportLayout.RightCameraWidthFraction, 1f);
        _layerCamera.depth = 1f;

        int activeLayerLayer = LayerMask.NameToLayer(_activeLayerName);
        if (activeLayerLayer < 0) throw new InvalidOperationException($"Слой '{_activeLayerName}' не найден.");

        int decorLayer = LayerMask.NameToLayer(_decorLayerName);
        if (decorLayer < 0)
        {
            // Декоративный слой не обязателен (ghost убран) — если нет, просто игнорируем.
            _layerCamera.cullingMask = 1 << activeLayerLayer;
        }
        else
        {
            _layerCamera.cullingMask = (1 << activeLayerLayer) | (1 << decorLayer);
        }

        _layerPresenter.LayoutChanged += UpdateCamera;
        _viewState.LayerRollChanged += UpdateCamera;
        _viewState.AxisChanged += UpdateCamera;
        _viewState.ActiveLayerChanged += UpdateCamera;
        _layerPresenter.Rebuilt += UpdateCamera;
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        var active = FindActiveContainer();
        if (active == null) return;

        Square gridSize = _boardLoader.GridSize;
        float step = _boardLoader.Step;

        int axis = _viewState.AxisIndex;
        Vector3 activeSize = GridAxes.ThinnedWorldSize(axis, gridSize, step);
        Vector3 activeCenter = active.TargetWorldPosition;
        Bounds bbox = new(activeCenter, activeSize);

        Vector3 axisDir = GridAxes.ToWorldDirection(axis);
        int viewDir = _viewState.ViewDirection;
        Vector3 cameraDir = axisDir * viewDir;
        Vector3 up = LayerLayout.ComputeLayerCameraUp(axis, viewDir, _viewState.LayerRollDegrees);
        Vector3 right = Vector3.Cross(cameraDir, up).normalized;

        Vector3 bboxCenter = bbox.center;
        Vector3 bboxExtents = bbox.extents;
        float halfW = 0f, halfH = 0f;
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sy = -1; sy <= 1; sy += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    Vector3 corner = bboxCenter + new Vector3(sx * bboxExtents.x, sy * bboxExtents.y, sz * bboxExtents.z);
                    Vector3 d = corner - activeCenter;
                    halfW = Mathf.Max(halfW, Mathf.Abs(Vector3.Dot(right, d)));
                    halfH = Mathf.Max(halfH, Mathf.Abs(Vector3.Dot(up, d)));
                }

        float vFov = _layerCamera.fieldOfView * Mathf.Deg2Rad;
        float hFov = 2f * Mathf.Atan(Mathf.Tan(vFov * 0.5f) * _layerCamera.aspect);
        float dV = halfH / Mathf.Tan(vFov * 0.5f);
        float dH = halfW / Mathf.Tan(hFov * 0.5f);
        float distance = Mathf.Max(dV, dH) * _padding;

        _layerCamera.transform.SetPositionAndRotation(
            activeCenter - cameraDir * distance,
            Quaternion.LookRotation(cameraDir, up));
    }

    private LayerContainer? FindActiveContainer()
    {
        int activeIndex = _viewState.ActiveLayerIndex;
        foreach (var c in _layerPresenter.Containers)
            if (c.LayerIndex == activeIndex) return c;
        return null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_layerPresenter != null)
        {
            _layerPresenter.LayoutChanged -= UpdateCamera;
            _layerPresenter.Rebuilt -= UpdateCamera;
        }
        if (_viewState != null)
        {
            _viewState.LayerRollChanged -= UpdateCamera;
            _viewState.AxisChanged -= UpdateCamera;
            _viewState.ActiveLayerChanged -= UpdateCamera;
        }
    }

    private void OnDestroy() => Dispose();
}
