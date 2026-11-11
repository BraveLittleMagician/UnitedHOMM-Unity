#nullable enable
using System;
using System.Collections.Generic;

public sealed class LayerVisibilityPresenter
{
    private readonly IReadOnlyList<LayerContainer> _containers;

    public LayerVisibilityPresenter(IReadOnlyList<LayerContainer> containers)
        => _containers = containers ?? throw new ArgumentNullException(nameof(containers));

    public void Apply(int activeIndex, int viewDir, int layersLayer, int activeLayerLayer)
    {
        foreach (var container in _containers)
        {
            bool isActive = container.LayerIndex == activeIndex;
            int targetLayer = isActive ? activeLayerLayer : layersLayer;
            container.SetVisibility(targetLayer);
        }
    }
}