#nullable enable

using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LayerSizeSetter : MonoBehaviour, ILayerSizeSetter
{
    private Mesh _mesh = null!;
    private Vector3 _initialScale = new();
    private Vector3 _initialMeshSize = new();
    private Renderer _renderer = null!;

    private void Awake()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        if (_mesh == null) throw new ArgumentNullException(nameof(_mesh));
        _renderer = GetComponent<Renderer>();
        if (_renderer == null) throw new ArgumentNullException(nameof(_renderer));
        _initialScale = transform.localScale;
        _initialMeshSize = _mesh.bounds.size;
    }

    public void SetSize(int width, int height, float cellSize, int layerIndex)
    {
        float worldWidth = width * cellSize;
        float worldHeight = height * cellSize;

        Vector3 targetScale;
        targetScale = new Vector3(worldWidth / _initialMeshSize.x, 1f, worldHeight / _initialMeshSize.z) * _initialScale.x;
        transform.localScale = targetScale;
        _renderer.material.SetVector("_CellSize", new Vector2(width / 2, height / 2));
        _renderer.material.SetFloat("_Reverse", layerIndex % 2 == 0 ? 0 : 1);
    }
}