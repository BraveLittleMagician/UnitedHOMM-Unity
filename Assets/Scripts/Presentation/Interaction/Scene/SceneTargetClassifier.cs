#nullable enable
using System;
using UnityEngine;

public sealed class SceneTargetClassifier
{
    private readonly Camera _camera;
    private readonly Camera _layerCamera;
    private readonly ViewState _viewState;
    private readonly LayerMask _pointerLayers;
    private readonly float _maxRayDistance;
    private readonly int _wwOffset;
    private readonly RaycastHit[] _hits = new RaycastHit[32];

    public SceneTargetClassifier(
        Camera camera,
        Camera layerCamera,
        ViewState viewState,
        LayerMask pointerLayers,
        float maxRayDistance,
        int wwOffset)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _layerCamera = layerCamera ?? throw new ArgumentNullException(nameof(layerCamera));
        _viewState = viewState ?? throw new ArgumentNullException(nameof(viewState));
        _pointerLayers = pointerLayers;
        _maxRayDistance = maxRayDistance;
        _wwOffset = wwOffset;
    }

    public SceneTargetInfo Classify(Vector2 screenPosition)
    {
        var camera = GetPointerCamera(screenPosition);
        bool isLayerView = camera == _layerCamera;
        return FindBestTarget(screenPosition, camera, isLayerView);
    }

    public bool TryClassify(Transform hitTransform, bool isLayerView, out int priority, out SceneTargetInfo info)
    {
        priority = int.MaxValue;
        info = SceneTargetInfo.None;

        // 1. W-куб (кликабелен только из main-camera; в layer-view фон не нужен)
        var wLayerBg = hitTransform.GetComponentInParent<WLayerBackground>();
        if (wLayerBg != null)
        {
            if (isLayerView) return false;
            priority = 4;
            info = new SceneTargetInfo(
                InteractionTargetKind.WLayer, null, null, null, null, wLayerBg.LayerIndex);
            return true;
        }

        // 2. Маркеры и фигуры — только в активном W-кубе
        var targetMarker = hitTransform.GetComponentInParent<TargetTargetMarker>();
        if (targetMarker != null)
        {
            if (!IsInActiveW(targetMarker.Cell)) return false;
            if (isLayerView && !IsOnActiveLayer(targetMarker.Cell)) return false;
            priority = 0;
            info = new SceneTargetInfo(InteractionTargetKind.TargetMarker, null, null, null, targetMarker);
            return true;
        }

        var attackMarker = hitTransform.GetComponentInParent<AttackMarker>();
        if (attackMarker != null)
        {
            if (!IsInActiveW(attackMarker.Cell)) return false;
            if (isLayerView && !IsOnActiveLayer(attackMarker.Cell)) return false;
            priority = 1;
            info = new SceneTargetInfo(InteractionTargetKind.AttackTarget, null, null, attackMarker, null);
            return true;
        }

        var moveTarget = hitTransform.GetComponentInParent<MoveTargetMarker>();
        if (moveTarget != null)
        {
            if (!IsInActiveW(moveTarget.Cell)) return false;
            if (isLayerView && !IsOnActiveLayer(moveTarget.Cell)) return false;
            priority = 2;
            info = new SceneTargetInfo(InteractionTargetKind.MoveTarget, null, moveTarget, null, null);
            return true;
        }

        var piece = hitTransform.GetComponentInParent<InfoOfPiece>();
        if (piece != null)
        {
            if (!IsInActiveW(piece.PositionInGrid)) return false;
            if (isLayerView && !IsOnActiveLayer(piece.PositionInGrid)) return false;
            priority = 3;
            info = new SceneTargetInfo(InteractionTargetKind.Piece, piece, null, null, null);
            return true;
        }

        return false;
    }

    private Camera GetPointerCamera(Vector2 screenPosition)
    {
        if (_camera.pixelRect.Contains(screenPosition)) return _camera;
        return _layerCamera;
    }

    private bool IsOnActiveLayer(Square cell)
    {
        int layerIndex = GridAxes.GetLayerIndex(_viewState.AxisIndex, cell, _wwOffset);
        return layerIndex == _viewState.ActiveLayerIndex;
    }

    private bool IsInActiveW(Square cell)
    {
        int wIndex = GridAxes.GetLayerIndex(GridAxes.W, cell, _wwOffset);
        return wIndex == _viewState.ActiveWIndex;
    }

    private SceneTargetInfo FindBestTarget(Vector2 screenPosition, Camera camera, bool isLayerView)
    {
        Ray ray = camera.ScreenPointToRay(screenPosition);
        int count = Physics.RaycastNonAlloc(ray, _hits, _maxRayDistance, _pointerLayers);
        if (count == 0) return SceneTargetInfo.None;

        int bestPriority = int.MaxValue;
        float bestDistance = float.MaxValue;
        SceneTargetInfo best = SceneTargetInfo.None;
        bool found = false;

        for (int i = 0; i < count; i++)
        {
            var hit = _hits[i];
            if (!TryClassify(hit.transform, isLayerView, out int priority, out var info))
                continue;
            if (!found
                || priority < bestPriority
                || (priority == bestPriority && hit.distance < bestDistance))
            {
                bestPriority = priority;
                bestDistance = hit.distance;
                best = info;
                found = true;
            }
        }
        return best;
    }
}