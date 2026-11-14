#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LayerVisibilityPresenter
{
    private readonly IReadOnlyList<LayerContainer> _containers;

    public LayerVisibilityPresenter(IReadOnlyList<LayerContainer> containers)
        => _containers = containers ?? throw new ArgumentNullException(nameof(containers));

    public void Apply(int activeWIndex, int axisIndex, int activeLayerIndex, int wwOffset, int layersLayer, int activeLayerLayer)
    {
        foreach (var container in _containers)
        {
            bool isActiveW = container.LayerIndex == activeWIndex;

            if (!isActiveW)
            {
                container.SetVisibilityRecursively(layersLayer);
                container.SetWLayerColliderEnabled(true);
                continue;
            }

            container.SetVisibilitySelf(layersLayer);
            container.SetWLayerColliderEnabled(false);

            foreach (Transform child in container.transform)
            {
                if (!child.TryGetComponent<InfoOfPiece>(out var info)) continue;
                int coord = GridAxes.GetLayerIndex(axisIndex, info.PositionInGrid, wwOffset);
                int targetLayer = coord == activeLayerIndex ? activeLayerLayer : layersLayer;
                child.gameObject.SetLayerRecursively(targetLayer);
            }

            if (container.TargetTwin != null)
            {
                foreach (Transform child in container.TargetTwin)
                {
                    Square cell;
                    if (child.TryGetComponent<MoveTargetMarker>(out var mv))
                        cell = mv.Cell;
                    else if (child.TryGetComponent<AttackMarker>(out var at))
                        cell = at.Cell;
                    else if (child.TryGetComponent<TargetTargetMarker>(out var tt))
                        cell = tt.Cell;
                    else
                        continue;

                    int coord = GridAxes.GetLayerIndex(axisIndex, cell, wwOffset);
                    int targetLayer = coord == activeLayerIndex ? activeLayerLayer : layersLayer;
                    child.gameObject.SetLayerRecursively(targetLayer);
                }
            }
        }
    }
}