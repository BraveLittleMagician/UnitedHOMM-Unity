#nullable enable

using System.Collections.Generic;
using UnityEngine;

public class UIPointerProbe : MonoBehaviour
{
    private readonly List<Rect> _regions = new();

    public void RegisterRegion(Rect guiRect)
    {
        for (int i = 0; i < _regions.Count; i++)
            if (_regions[i] == guiRect) return;
        _regions.Add(guiRect);
    }

    public void UnregisterRegion(Rect guiRect)
    {
        for (int i = _regions.Count - 1; i >= 0; i--)
            if (_regions[i] == guiRect) _regions.RemoveAt(i);
    }

    public void ClearRegions() => _regions.Clear();

    public bool IsPointerOverUI(Vector2 screenPosition)
    {
        float guiY = Screen.height - screenPosition.y;
        Vector2 guiPos = new(screenPosition.x, guiY);

        for (int i = 0; i < _regions.Count; i++)
            if (_regions[i].Contains(guiPos)) return true;

        return false;
    }
}