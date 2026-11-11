#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LayerPositioner
{
    private readonly IReadOnlyList<LayerContainer> _containers;

    public LayerPositioner(IReadOnlyList<LayerContainer> containers)
        => _containers = containers ?? throw new ArgumentNullException(nameof(containers));

    public void ApplyPositions(Square gridSize, float step, float layerGap)
    {
        foreach (var container in _containers)
        {
            Vector3 pos = LayerLayout.ComputeBasePosition(
                container.Axis, container.LayerIndex, gridSize, step, layerGap);
            container.MoveTo(pos);
        }
    }

    public void UpdateSizes(Square gridSize, float step)
    {
        foreach (var container in _containers)
            container.UpdateGridSize(gridSize, step);
    }
}