#nullable enable
using UnityEngine;
using Object = UnityEngine.Object;

public static class UnityObjectCleanup
{
    public static void DestroySafe(Object? obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Object.Destroy(obj);
        else Object.DestroyImmediate(obj);
    }
}