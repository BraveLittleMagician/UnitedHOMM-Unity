#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PieceVisual : MonoBehaviour
{
    private static readonly int _normalColorId = Shader.PropertyToID("_NormalColor");
    private static readonly int _isSelectedId = Shader.PropertyToID("_IsSelected");
    private static readonly int _isHighlightedId = Shader.PropertyToID("_IsHighlighted");
    private static readonly int _isHoveredId = Shader.PropertyToID("_IsHovered");

    private MaterialPropertyBlock? _block;
    private Renderer[] _renderers = Array.Empty<Renderer>();
    private Material _pieceMaterial = null!;
    private Color _teamColor;
    private bool _initialized;

    public void Initialize(Material pieceMaterial)
    {
        if (pieceMaterial == null) throw new ArgumentNullException(nameof(pieceMaterial));
        if (_initialized) return;

        _pieceMaterial = pieceMaterial;
        _block = new MaterialPropertyBlock();
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        if (_renderers.Length == 0)
            throw new InvalidOperationException($"У '{name}' нет ни одного Renderer — цвет применить нельзя.");

        var materials = new List<Material>(1) { _pieceMaterial };
        foreach (var r in _renderers)
        {
            r.SetMaterials(materials);
            r.SetPropertyBlock(_block);
        }
        _initialized = true;
    }

    public void ApplyTeamColor(Color color)
    {
        if (!_initialized || _block == null)
            throw new InvalidOperationException($"PieceVisual на '{name}' не инициализирован.");
        _teamColor = color;
        _block.SetColor(_normalColorId, color);
        Push();
    }

    public void SetState(bool selected, bool highlighted, bool hovered)
    {
        if (!_initialized || _block == null) return;
        _block.SetFloat(_isSelectedId, selected ? 1f : 0f);
        _block.SetFloat(_isHighlightedId, highlighted ? 1f : 0f);
        _block.SetFloat(_isHoveredId, hovered ? 1f : 0f);
        Push();
    }

    public void ReplaceVisual(GameObject prefab, Color color)
    {
        if (prefab == null) throw new ArgumentNullException(nameof(prefab));
        if (!_initialized || _block == null)
            throw new InvalidOperationException($"PieceVisual на '{name}' не инициализирован.");

        int targetLayer = gameObject.layer;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            child.SetActive(false);
            UnityObjectCleanup.DestroySafe(child);
        }
        foreach (var r in GetComponents<Renderer>()) { r.enabled = false; UnityObjectCleanup.DestroySafe(r); }
        foreach (var mf in GetComponents<MeshFilter>()) UnityObjectCleanup.DestroySafe(mf);
        foreach (var c in GetComponents<Collider>()) { c.enabled = false; UnityObjectCleanup.DestroySafe(c); }

        var instance = Instantiate(prefab, transform);
        instance.name = "Visual";
        instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        instance.transform.localScale = Vector3.one;
        instance.SetLayerRecursively(targetLayer);

        _renderers = instance.GetComponentsInChildren<Renderer>(includeInactive: true);
        if (_renderers.Length == 0)
            throw new InvalidOperationException($"У префаба '{prefab.name}' нет ни одного Renderer.");

        var materials = new List<Material>(1) { _pieceMaterial };
        foreach (var r in _renderers)
        {
            r.SetMaterials(materials);
            r.SetPropertyBlock(_block);
        }

        _teamColor = color;
        _block.SetColor(_normalColorId, color);
        Push();
    }

    private void Push()
    {
        if (!_initialized || _block == null) return;
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            if (r == null) continue;
            r.SetPropertyBlock(_block);
        }
    }
}