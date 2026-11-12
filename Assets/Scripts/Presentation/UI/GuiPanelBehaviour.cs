#nullable enable
using System;
using UnityEngine;

public abstract class GuiPanelBehaviour : EventSubscriberBehaviour
{
    private UIPointerProbe? _probe;
    private Rect _panelRect;
    private Vector2Int _lastScreenSize;
    private bool _registered;

    protected Rect PanelRect => _panelRect;
    protected bool IsRegistered => _registered;

    protected void InitializePanel(UIPointerProbe probe)
    {
        _probe = probe ?? throw new ArgumentNullException(nameof(probe));
    }

    protected void RegisterPanel()
    {
        if (_probe == null)
            throw new InvalidOperationException($"{GetType().Name}.RegisterPanel: InitializePanel не был вызван.");
        if (_registered) return;

        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        _panelRect = ClampToViewport(
            ComputePanelRect(_lastScreenSize.x, _lastScreenSize.y),
            _lastScreenSize.x, _lastScreenSize.y);
        _probe.RegisterRegion(_panelRect);
        _registered = true;
    }

    protected void EnsurePanelRectCurrent()
    {
        if (!_registered || _probe == null) return;
        var size = new Vector2Int(Screen.width, Screen.height);
        if (size == _lastScreenSize) return;
        _lastScreenSize = size;
        var newRect = ClampToViewport(ComputePanelRect(size.x, size.y), size.x, size.y);
        if (newRect == _panelRect) return;
        _probe.TryReplaceRegion(_panelRect, newRect);
        _panelRect = newRect;
    }

    protected abstract Rect ComputePanelRect(int screenWidth, int screenHeight);

    protected override void OnCleanup()
    {
        if (!_registered || _probe == null) return;
        _probe.UnregisterRegion(_panelRect);
        _registered = false;
    }

    private static Rect ClampToViewport(Rect rect, int screenWidth, int screenHeight)
    {
        float width = Mathf.Clamp(rect.width, 1f, Mathf.Max(1, screenWidth));
        float height = Mathf.Clamp(rect.height, 1f, Mathf.Max(1, screenHeight));
        float x = Mathf.Clamp(rect.x, 0f, Mathf.Max(0f, screenWidth - width));
        float y = Mathf.Clamp(rect.y, 0f, Mathf.Max(0f, screenHeight - height));
        return new Rect(x, y, width, height);
    }
}
