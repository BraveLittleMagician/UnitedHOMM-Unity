#nullable enable

using System;
using UnityEngine;

public class DeselectOnEmptyClick : EventSubscriberBehaviour, IClickLeftSubscriber, IPointerSubscriber, IRunnable
{
    private ISelectionHub _selectionHub = null!;
    private IClickLeftHub _clickLeftHub = null!;
    private IPointerHub _pointerHub = null!;
    private UIPointerProbe _uiPointerProbe = null!;
    private Camera _camera = null!;

    [SerializeField] private LayerMask _clickableLayers = ~0;
    [SerializeField] private float _maxRayDistance = 1000f;

    private Vector2 _lastPointerPosition;

    public void Initialize(ISelectionHub selectionHub, IClickLeftHub clickLeftHub, IPointerHub pointerHub, UIPointerProbe uiPointerProbe, Camera camera)
    {
        _selectionHub = selectionHub ?? throw new ArgumentNullException(nameof(selectionHub));
        _clickLeftHub = clickLeftHub ?? throw new ArgumentNullException(nameof(clickLeftHub));
        _pointerHub = pointerHub ?? throw new ArgumentNullException(nameof(pointerHub));
        _uiPointerProbe = uiPointerProbe ?? throw new ArgumentNullException(nameof(uiPointerProbe));
        _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
    }
    public void Run()
    {
        _clickLeftHub.RegisterSubscriber(this);
        AddSubscription(() => _clickLeftHub.UnregisterSubscriber(this));

        _pointerHub.RegisterSubscriber(this);
        AddSubscription(() => _pointerHub.UnregisterSubscriber(this));
    }
    public void Point(Vector2 pointer)
    {
        _lastPointerPosition = pointer;
    }
    public void TriggerClickLeft()
    {
        if (_uiPointerProbe.IsPointerOverUI(_lastPointerPosition)) return;

        Ray ray = _camera.ScreenPointToRay(_lastPointerPosition);
        if (Physics.Raycast(ray, out _, _maxRayDistance, _clickableLayers)) return;

        _selectionHub.DropSelection();
    }
}