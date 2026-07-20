#nullable enable

using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LayerSizeSetter : MonoBehaviour, ILayerSizeSetter
{
    [Header("Settings")]
    [SerializeField] private float _extraWidthPadding = 0f;
    [SerializeField] private float _extraHeightPadding = 0f;
    [SerializeField] private bool _useLocalScale = true;

    private Mesh _mesh = null!;
    private Vector3 _initialScale = new();
    private Vector3 _initialMeshSize = new();

    private void Awake()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        if (_mesh == null) throw new ArgumentNullException(nameof(_mesh));
        _initialScale = transform.localScale;
        _initialMeshSize = _mesh.bounds.size;
    }

    public void SetSize(int width, int height, float cellSize)
    {
        if (_mesh == null) return;

        float worldWidth = width * cellSize + _extraWidthPadding;
        float worldHeight = height * cellSize + _extraHeightPadding;

        Vector3 targetScale;
        if (_useLocalScale)
        {
            targetScale = new Vector3(
                worldWidth / _initialMeshSize.x,
                1f,
                worldHeight / _initialMeshSize.z
            ) * _initialScale.x;
        }
        else
        {
            Debug.LogWarning("Изменение размера меша через вершины не реализовано. Используйте LocalScale.");
            targetScale = transform.localScale;
        }

        transform.localScale = targetScale;
    }
}