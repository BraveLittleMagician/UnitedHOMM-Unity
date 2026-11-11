#nullable enable

using UnityEngine;

public static class GameObjectExtensions
{
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
    public static Vector3 ToWorldCenter(this Square size, float cellSize = 1f)
    {
        return new Vector3(
            (size.X - 1) / 2f * cellSize,
            (size.Z - 1) / 2f * cellSize,
            (size.Y - 1) / 2f * cellSize);
    }
}