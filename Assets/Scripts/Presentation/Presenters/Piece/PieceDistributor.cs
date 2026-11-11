#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PieceDistributor
{
    private readonly IReadOnlyList<LayerContainer> _containers;
    private readonly PieceViewRegistry _viewRegistry;
    private readonly LoaderOfBoard _boardLoader;

    public PieceDistributor(
        IReadOnlyList<LayerContainer> containers,
        PieceViewRegistry viewRegistry,
        LoaderOfBoard boardLoader)
    {
        _containers = containers ?? throw new ArgumentNullException(nameof(containers));
        _viewRegistry = viewRegistry ?? throw new ArgumentNullException(nameof(viewRegistry));
        _boardLoader = boardLoader ?? throw new ArgumentNullException(nameof(boardLoader));
    }

    public void DistributeAll(int axis)
    {
        foreach (var view in _viewRegistry.AllLiveViews)
        {
            if (view == null) continue;
            int layerIndex = GridAxes.GetGridCoordinate(axis, view.PositionInGrid);
            if (layerIndex < 0 || layerIndex >= _containers.Count) continue;
            var container = _containers[layerIndex];
            container.AddPiece(view.gameObject);
            PositionPieceInContainer(view, container);
        }
    }

    public void ReattachAll(Transform sourceParent, int axis)
    {
        foreach (var view in _viewRegistry.AllLiveViews)
        {
            if (view == null) continue;
            view.transform.SetParent(sourceParent, false);
        }
        foreach (var container in _containers)
            container.ResetPieceCount();
        DistributeAll(axis);
    }

    public void RepositionInContainers()
    {
        foreach (var container in _containers)
        {
            foreach (Transform child in container.transform)
            {
                if (!child.TryGetComponent<InfoOfPiece>(out var info)) continue;
                if (info.IsDead) continue;
                PositionPieceInContainer(info, container);
            }
        }
    }

    public void DistributeView(InfoOfPiece view, int axis)
    {
        if (view == null || view.IsDead) return;
        int layerIndex = GridAxes.GetGridCoordinate(axis, view.PositionInGrid);
        if (layerIndex < 0 || layerIndex >= _containers.Count) return;
        var container = _containers[layerIndex];
        container.AddPiece(view.gameObject);
        PositionPieceInContainer(view, container);
    }

    public void PositionPieceInContainer(InfoOfPiece info, LayerContainer container)
    {
        Vector3 worldLocal = GridAxes.GridToWorldLocal(
            info.PositionInGrid, _boardLoader.GridSize, _boardLoader.Step, _boardLoader.WSpread);
        Vector3 local = worldLocal - container.TargetLocalPosition;

        info.transform.SetParent(container.transform, false);
        info.transform.localPosition = local;
    }
}