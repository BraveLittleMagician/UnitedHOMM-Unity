#nullable enable
using UnityEngine;

public sealed class BoardViewFactoryContext
{
    public int Axis { get; init; }
    public int LayersLayerIndex { get; init; }
    public float Step { get; init; }
    public float LayerGap { get; init; }
    public string AxisName { get; init; } = null!;
    public Square GridSize { get; init; }
    public Transform Parent { get; init; } = null!;
    public Material LayerMaterial { get; init; } = null!;
}