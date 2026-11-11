#nullable enable
using System;
using UnityEngine;

public class WorldToScreenMarker : MonoBehaviour
{
    private Camera _camera = null!;
    private Vector3 _worldPos;
    private bool _hasWorldPos;
    private bool _userVisible;
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    private void LateUpdate()
    {
        if (Screen.width == _lastScreenWidth && Screen.height == _lastScreenHeight) return;
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
        Refresh();
    }

    public void Initialize(Camera camera)
    {
        _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
    }

    public void Refresh()
    {
        if (_hasWorldPos && _camera != null)
        {
            Vector3 screenPos = _camera.WorldToScreenPoint(_worldPos);
            if (screenPos.z >= 0f) transform.position = screenPos;
        }
        ApplyVisibility();
    }

    public void SetWorldPosition(Vector3 worldPos)
    {
        if (_camera == null) throw new InvalidOperationException("WorldToScreenMarker не инициализирован.");
        _worldPos = worldPos;
        _hasWorldPos = true;
        Refresh();
    }

    public void SetUserVisible(bool visible)
    {
        _userVisible = visible;
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        bool onScreen = _hasWorldPos && _camera != null && _camera.WorldToScreenPoint(_worldPos).z >= 0f;
        gameObject.SetActive(_userVisible && onScreen);
    }
}