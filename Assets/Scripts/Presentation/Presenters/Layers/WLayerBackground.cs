#nullable enable
using UnityEngine;

public sealed class WLayerBackground : MonoBehaviour
{
    public int LayerIndex { get; private set; }

    public void Initialize(int layerIndex) => LayerIndex = layerIndex;
}