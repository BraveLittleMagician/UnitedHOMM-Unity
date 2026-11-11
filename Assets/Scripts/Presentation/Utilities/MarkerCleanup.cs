#nullable enable

using System.Collections.Generic;
using UnityEngine;

public static class MarkerCleanup
{
    public static void DestroyMarkers(List<GameObject> markers)
    {
        for (int i = markers.Count - 1; i >= 0; i--)
        {
            var go = markers[i];
            if (go == null) continue;
            go.transform.SetParent(null, false);
            UnityObjectCleanup.DestroySafe(go);
        }
        markers.Clear();
    }
}