#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class MoveTargetPresenter : IMarkerPresenter
{
    private readonly float _markerSizeFraction;
    private readonly ViewState _viewState;
    private readonly LayerPresenter _layerPresenter;
    private readonly LoaderOfBoard _boardLoader;
    private readonly Material _markerMaterial;
    private readonly List<GameObject> _markers = new();

    public MoveTargetPresenter(
        ViewState viewState,
        LayerPresenter layerPresenter,
        LoaderOfBoard boardLoader,
        Material markerMaterial,
        float markerSizeFraction)
    {
        _viewState = viewState ?? throw new ArgumentNullException(nameof(viewState));
        _layerPresenter = layerPresenter ?? throw new ArgumentNullException(nameof(layerPresenter));
        _boardLoader = boardLoader ?? throw new ArgumentNullException(nameof(boardLoader));
        _markerMaterial = markerMaterial ?? throw new ArgumentNullException(nameof(markerMaterial));
        _markerSizeFraction = markerSizeFraction;
    }

    public void Show(IReadOnlyList<ShapeCell> shape)
    {
        if (shape == null) throw new ArgumentNullException(nameof(shape));

        float markerSize = _boardLoader.Step * _markerSizeFraction;
        int axisIndex = _viewState.AxisIndex;
        Mesh markerMesh = BoxMeshCache.Get(Vector3.one * markerSize);

        foreach (var shapeCell in shape)
        {
            var cell = shapeCell.Cell;
            int layerIndex = GridAxes.GetGridCoordinate(axisIndex, cell);
            var parentLayer = _layerPresenter.GetMoveTargetLayer(layerIndex);
            if (parentLayer == null) continue;

            var go = new GameObject($"{cell.X}_{cell.Y}_{cell.Z}_{cell.W}_MoveTarget");
            go.transform.SetParent(parentLayer, false);
            go.transform.localPosition = ComputeLocalPosition(cell, parentLayer.localPosition, axisIndex);
            go.layer = parentLayer.gameObject.layer;

            var col = go.AddComponent<BoxCollider>();
            col.size = Vector3.one * markerSize;
            col.center = Vector3.zero;

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = markerMesh;
            var rend = go.AddComponent<MeshRenderer>();
            rend.sharedMaterial = _markerMaterial;
            rend.shadowCastingMode = ShadowCastingMode.Off;
            rend.receiveShadows = false;

            var block = new MaterialPropertyBlock();
            block.SetFloat("_IsHovered", 0f);
            block.SetFloat("_IsStay", 0f);
            block.SetFloat("_IsAttack", 0f);

            var marker = go.AddComponent<MoveTargetMarker>();
            marker.Initialize(cell, shapeCell.Stayable, block);
            _markers.Add(go);
        }
    }

    public void Reposition()
    {
        if (_markers.Count == 0) return;

        float step = _boardLoader.Step;
        float markerSize = step * _markerSizeFraction;
        Mesh markerMesh = BoxMeshCache.Get(Vector3.one * markerSize);
        Vector3 colliderSize = Vector3.one * markerSize;
        int axisIndex = _viewState.AxisIndex;

        foreach (var go in _markers)
        {
            if (go == null) continue;
            if (!go.TryGetComponent<MoveTargetMarker>(out var marker)) continue;
            var parent = go.transform.parent;
            if (parent == null) continue;

            go.transform.localPosition = ComputeLocalPosition(marker.Cell, parent.localPosition, axisIndex);
            if (go.TryGetComponent<BoxCollider>(out var col)) col.size = colliderSize;
            if (go.TryGetComponent<MeshFilter>(out var mf)) mf.sharedMesh = markerMesh;
        }
    }

    public void Clear() => MarkerCleanup.DestroyMarkers(_markers);

    private Vector3 ComputeLocalPosition(Square cell, Vector3 containerLocalPos, int axisIndex)
    {
        Vector3 worldLocal = GridAxes.GridToWorldLocal(
            cell, _boardLoader.GridSize, _boardLoader.Step, _boardLoader.WSpread);
        Vector3 local = worldLocal - containerLocalPos;

        // Для не-W осей обнуляем компоненту вдоль оси слоя —
        // иначе маркер «уедет» на двойной offset.
        if (axisIndex != GridAxes.W)
        {
            int worldComp = GridAxes.GetWorldComponent(axisIndex);
            if (worldComp >= 0 && worldComp < 3) local[worldComp] = 0f;
        }
        return local;
    }
}