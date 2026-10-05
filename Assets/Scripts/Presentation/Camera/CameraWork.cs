#nullable enable

using System;
using UnityEngine;

public class CameraWork : EventSubscriberBehaviour, IRunnable, IHoldRightSubscriber, IHoldMiddleSubscriber, IDeltaSubscriber, IScrollSubscriber
{
    [SerializeField] private float _distance = 15f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _panSpeed = 1f;
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float _minDistance = 5f;
    [SerializeField] private float _maxDistance = 60f;
    [SerializeField] private float _snapDistance = 5f;

    private Camera _camera = null!;
    private Transform _boardRoot = null!;
    private HoldRight _holdRightHub = null!;
    private HoldMiddle _holdMiddleHub = null!;
    private Delta _deltaHub = null!;
    private Scroll _scrollHub = null!;
    private WorldToScreenMarker _cubeCenterMarker = null!;
    private WorldToScreenMarker _rotationCenterMarker = null!;

    private float _x = 0f;
    private float _y = 0f;
    private bool _rightHolding = false;
    private bool _middleHolding = false;
    private Vector3 _targetOffset = Vector3.zero;

    public Vector3 RotationCenter => _boardRoot.position + transform.position + _targetOffset;

    private void UpdatePosition()
    {
        Quaternion rotation = Quaternion.Euler(_y, _x, 0);
        Vector3 negDistance = new(0, 0, -_distance);
        Vector3 position = rotation * negDistance + RotationCenter;

        transform.SetPositionAndRotation(position, rotation);

        UpdateMarkers();
    }
    private void UpdateMarkers()
    {
        _cubeCenterMarker.SetWorldPosition(_boardRoot.position);
        _rotationCenterMarker.SetWorldPosition(RotationCenter);

        _cubeCenterMarker.Refresh();
        _rotationCenterMarker.Refresh();
    }
    private void SetMarkersVisible(bool visible)
    {
        _cubeCenterMarker.gameObject.SetActive(visible);
        _rotationCenterMarker.gameObject.SetActive(visible);
    }
    private bool IsCloseToCenterOnScreen()
    {
        if (_boardRoot == null) return false;

        Vector3 centerWorld = _boardRoot.position;
        Vector3 rotationCenterWorld = RotationCenter;

        Vector3 screenCenter = _camera.WorldToScreenPoint(centerWorld);
        Vector3 screenRotation = _camera.WorldToScreenPoint(rotationCenterWorld);

        if (screenCenter.z < 0 || screenRotation.z < 0) return false;

        Vector2 diff = new(screenCenter.x - screenRotation.x, screenCenter.y - screenRotation.y);
        return diff.magnitude <= _snapDistance;
    }

    public void Delta(Vector2 delta)
    {
        if (_middleHolding)
        {
            Vector3 right = transform.right;
            Vector3 up = transform.up;
            _targetOffset -= _panSpeed * 0.02f * (right * delta.x + up * delta.y);

            if (IsCloseToCenterOnScreen()) _targetOffset = Vector3.zero;

            UpdatePosition();
        }
        else if (_rightHolding)
        {
            _x += delta.x * _rotationSpeed * 0.02f;
            _y -= delta.y * _rotationSpeed * 0.02f;
            _y = Mathf.Clamp(_y, -90f, 90f);

            UpdatePosition();
        }
    }
    public void Scroll(float scrollDelta)
    {
        _distance -= scrollDelta * _zoomSpeed;
        _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);
        UpdatePosition();
    }
    public void TriggerHoldRightStarted()
    {
        _rightHolding = true;
        SetMarkersVisible(true);
    }
    public void TriggerHoldRightCanceled()
    {
        _rightHolding = false;
        SetMarkersVisible(false);
    }
    public void TriggerHoldMiddleStarted()
    {
        _middleHolding = true;
        SetMarkersVisible(true);
    }
    public void TriggerHoldMiddleCanceled()
    {
        _middleHolding = false;
        SetMarkersVisible(false);
    }
    public void Initialize(Transform piecesRoot, Camera camera, WorldToScreenMarker cubeCenterMarker, WorldToScreenMarker rotationCenterMarker, HoldRight holdRightHub, HoldMiddle holdMiddleHub, Delta deltaHub, Scroll scrollHub)
    {
        _boardRoot = piecesRoot != null ? piecesRoot : throw new ArgumentNullException(nameof(piecesRoot));
        _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
        _cubeCenterMarker = cubeCenterMarker != null ? cubeCenterMarker : throw new ArgumentNullException(nameof(cubeCenterMarker));
        _rotationCenterMarker = rotationCenterMarker != null ? rotationCenterMarker : throw new ArgumentNullException(nameof(rotationCenterMarker));
        _holdRightHub = holdRightHub != null ? holdRightHub : throw new ArgumentNullException(nameof(holdRightHub));
        _holdMiddleHub = holdMiddleHub != null ? holdMiddleHub : throw new ArgumentNullException(nameof(holdMiddleHub));
        _deltaHub = deltaHub != null ? deltaHub : throw new ArgumentNullException(nameof(deltaHub));
        _scrollHub = scrollHub != null ? scrollHub : throw new ArgumentNullException(nameof(scrollHub));
        _cubeCenterMarker.Initialize(_camera);
        _rotationCenterMarker.Initialize(_camera);
    }
    public void Run() 
    {
        Vector3 angles = transform.eulerAngles;
        _x = angles.y;
        _y = angles.x;

        _holdRightHub.RegisterSubscriber(this);
        AddSubscription(() => _holdRightHub.UnregisterSubscriber(this));

        _holdMiddleHub.RegisterSubscriber(this);
        AddSubscription(() => _holdMiddleHub.UnregisterSubscriber(this));

        _deltaHub.RegisterSubscriber(this);
        AddSubscription(() => _deltaHub.UnregisterSubscriber(this));

        _scrollHub.RegisterSubscriber(this);
        AddSubscription(() => _scrollHub.UnregisterSubscriber(this));

        SetMarkersVisible(false);
        UpdateMarkers();
    }
}