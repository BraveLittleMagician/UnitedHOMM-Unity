#nullable enable

using UnityEngine;

public static class GameObjectExtensions
{
    public static void SetSelectionState(this InfoOfPiece info, bool selected, bool highlighted, bool hovered)
    {
        var renderers = info.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        info.Block.SetFloat("_IsSelected", selected ? 1f : 0f);
        info.Block.SetFloat("_IsHighlighted", highlighted ? 1f : 0f);
        info.Block.SetFloat("_IsHovered", hovered ? 1f : 0f);
        foreach (var r in renderers) r.SetPropertyBlock(info.Block);
    }
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        return obj.TryGetComponent<T>(out var c) ? c : obj.AddComponent<T>();
    }
    public static void SetLayerRecursively(this GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            child.gameObject.SetLayerRecursively(layer);
    }
}