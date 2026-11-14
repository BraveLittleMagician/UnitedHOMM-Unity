#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class MarkerPresenterBase : IMarkerPresenter
{
    private readonly float _markerSizeFraction;
    private readonly LayerPresenter _layerPresenter;
    private readonly LoaderOfBoard _boardLoader;
    private readonly Material _markerMaterial;
    private readonly List<GameObject> _markers = new();

    protected MarkerPresenterBase(
        LayerPresenter layerPresenter,
        LoaderOfBoard boardLoader,
        Material markerMaterial,
        float markerSizeFraction)
    {
        _layerPresenter = layerPresenter ?? throw new ArgumentNullException(nameof(layerPresenter));
        _boardLoader = boardLoader ?? throw new ArgumentNullException(nameof(boardLoader));
        _markerMaterial = markerMaterial ?? throw new ArgumentNullException(nameof(markerMaterial));
        _markerSizeFraction = markerSizeFraction;
    }

    protected abstract string MarkerNameSuffix { get; }

    protected (GameObject go, MaterialPropertyBlock block)? CreateMarker(
        Square cell,
        Action<MaterialPropertyBlock>? configureBlock = null)
    {
        float markerSize = _boardLoader.Step * _markerSizeFraction;
        int wIndex = GridAxes.GetLayerIndex(GridAxes.W, cell, _boardLoader.WWOffset);
        var parentLayer = _layerPresenter.GetMoveTargetLayer(wIndex);
        if (parentLayer == null) return null;

        var go = new GameObject($"{cell.X}_{cell.Y}_{cell.Z}_{cell.W}_{MarkerNameSuffix}");
        go.transform.SetParent(parentLayer, false);
        go.transform.localPosition = ComputeLocalPosition(cell, parentLayer.localPosition);
        go.layer = parentLayer.gameObject.layer;

        var col = go.AddComponent<BoxCollider>();
        col.size = Vector3.one * markerSize;
        col.center = Vector3.zero;

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = BoxMeshCache.Get(Vector3.one * markerSize);

        var rend = go.AddComponent<MeshRenderer>();
        rend.sharedMaterial = _markerMaterial;
        rend.shadowCastingMode = ShadowCastingMode.Off;
        rend.receiveShadows = false;

        var block = new MaterialPropertyBlock();
        block.SetFloat("_IsHovered", 0f);
        configureBlock?.Invoke(block);
        rend.SetPropertyBlock(block);

        _markers.Add(go);
        return (go, block);
    }

    public void Reposition()
    {
        if (_markers.Count == 0) return;

        float markerSize = _boardLoader.Step * _markerSizeFraction;
        Mesh markerMesh = BoxMeshCache.Get(Vector3.one * markerSize);
        Vector3 colliderSize = Vector3.one * markerSize;

        foreach (var go in _markers)
        {
            if (go == null) continue;
            if (!go.TryGetComponent<IMarkerCell>(out var marker)) continue;
            var parent = go.transform.parent;
            if (parent == null) continue;

            go.transform.localPosition = ComputeLocalPosition(marker.Cell, parent.localPosition);
            if (go.TryGetComponent<BoxCollider>(out var col)) col.size = colliderSize;
            if (go.TryGetComponent<MeshFilter>(out var mf)) mf.sharedMesh = markerMesh;
        }
    }

    public void Clear() => MarkerCleanup.DestroyMarkers(_markers);

    private Vector3 ComputeLocalPosition(Square cell, Vector3 containerLocalPos)
    {
        Vector3 worldLocal = GridAxes.GridToWorldLocal(
            cell,
            _boardLoader.GridSize,
            _boardLoader.Step,
            _boardLoader.WSpread,
            _boardLoader.WWOffset);
        return worldLocal - containerLocalPos;
    }
}