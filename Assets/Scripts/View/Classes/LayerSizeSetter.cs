#nullable enable

using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LayerSizeSetter : MonoBehaviour, ILayerSizeSetter
{
    private Mesh _mesh = null!;
    private Renderer _renderer = null!;
    private MaterialPropertyBlock _propertyBlock = null!;

    private void Awake()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _renderer = GetComponent<Renderer>();
        if (_mesh == null) throw new ArgumentNullException(nameof(_mesh));
        if (_renderer == null) throw new ArgumentNullException(nameof(_renderer));
        _propertyBlock = new MaterialPropertyBlock();
    }

    public void SetSize(int width, int height, float cellSize, int layerIndex)
    {
        _renderer.GetPropertyBlock(_propertyBlock);

        float worldWidth = width * cellSize;
        float worldHeight = height * cellSize;

        Vector3 targetScale = new(worldWidth, worldHeight, 1);
        transform.localScale = targetScale;
        transform.localEulerAngles = new(90, 0, 0);

        _propertyBlock.SetVector("_CellSize", new Vector2(width / 2, height / 2));
        _propertyBlock.SetFloat("_Reverse", layerIndex % 2 == 0 ? 0 : 1);
        _renderer.SetPropertyBlock(_propertyBlock);
    }
}