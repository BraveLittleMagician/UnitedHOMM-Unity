#nullable enable
using System;
using UnityEngine;

public class CameraWork : EventSubscriberBehaviour, IRunnable,
    IHoldRightSubscriber, IHoldMiddleSubscriber, IDeltaSubscriber, IScrollSubscriber
{
    [SerializeField] private float _distance = 15f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _panSpeed = 1f;
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float _minDistance = 5f;
    [SerializeField] private float _maxDistance = 60f;
    [SerializeField] private float _snapDistance = 5f;

    private Camera _camera = null!;
    private LoaderOfBoard _boardLoader = null!;
    private IHoldRightHub _holdRightHub = null!;
    private IHoldMiddleHub _holdMiddleHub = null!;
    private IDeltaHub _deltaHub = null!;
    private IScrollHub _scrollHub = null!;
    private CameraManipulationState _manipulationState = null!;
    private CameraManipulationModel _model = null!;
    private CameraMarkerPresenter _markers = null!;

    private bool _rightHolding;
    private bool _middleHolding;

    private Vector3 RotationCenter => _boardLoader.transform.position + _model.TargetOffset;

    public void Initialize(CameraWorkContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _boardLoader = context.BoardLoader ?? throw new ArgumentNullException(nameof(context.BoardLoader));
        _camera = context.Camera ?? throw new ArgumentNullException(nameof(context.Camera));
        _holdRightHub = context.HoldRightHub ?? throw new ArgumentNullException(nameof(context.HoldRightHub));
        _holdMiddleHub = context.HoldMiddleHub ?? throw new ArgumentNullException(nameof(context.HoldMiddleHub));
        _deltaHub = context.DeltaHub ?? throw new ArgumentNullException(nameof(context.DeltaHub));
        _scrollHub = context.ScrollHub ?? throw new ArgumentNullException(nameof(context.ScrollHub));
        _manipulationState = context.ManipulationState ?? throw new ArgumentNullException(nameof(context.ManipulationState));

        _model = new CameraManipulationModel(_rotationSpeed, _panSpeed, _zoomSpeed, _minDistance, _maxDistance);
        _markers = new CameraMarkerPresenter(
            _camera, context.CubeCenterMarker, context.RotationCenterMarker, _snapDistance);
        _markers.Initialize();
    }

    public void Run()
    {
        if (_camera == null) throw new InvalidOperationException("CameraWork не инициализирован.");

        _camera.rect = new Rect(0f, 0f, ViewportLayout.LeftCameraWidthFraction, 1f);
        int decorLayer = LayerMask.NameToLayer("LayerViewDecor");
        if (decorLayer >= 0) _camera.cullingMask &= ~(1 << decorLayer);

        Vector3 angles = transform.eulerAngles;
        _model.Initialize(angles.y, angles.x, angles.z, _distance);

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

    private void UpdatePosition()
    {
        Quaternion rotation = _model.Rotation;
        Vector3 position = _model.GetPosition(RotationCenter);
        transform.SetPositionAndRotation(position, rotation);
        UpdateMarkers();
    }

    private void UpdateMarkers() => _markers.Update(_boardLoader.transform.position, RotationCenter);
    private void SetMarkersVisible(bool visible) => _markers.SetVisible(visible);

    private void UpdateHoldState()
    {
        bool anyHolding = _rightHolding || _middleHolding;
        SetMarkersVisible(anyHolding);
        _manipulationState.SetActive(anyHolding);
    }

    public void Delta(Vector2 delta)
    {
        if (_middleHolding)
        {
            _model.ApplyPanDelta(delta, transform.right, transform.up);
            if (_markers.IsCloseToCenterOnScreen(_boardLoader.transform.position, RotationCenter))
                _model.SnapPanToZero();
            UpdatePosition();
        }
        else if (_rightHolding)
        {
            _model.ApplyOrbitDelta(delta);
            UpdatePosition();
        }
    }

    public void Scroll(float scrollDelta)
    {
        _model.ApplyZoom(scrollDelta);
        UpdatePosition();
    }

    public void TriggerHoldRightStarted()
    {
        _rightHolding = true;
        UpdateHoldState();
    }

    public void TriggerHoldRightCanceled()
    {
        _rightHolding = false;
        UpdateHoldState();
    }

    public void TriggerHoldMiddleStarted()
    {
        _middleHolding = true;
        UpdateHoldState();
    }

    public void TriggerHoldMiddleCanceled()
    {
        _middleHolding = false;
        UpdateHoldState();
    }

    protected override void OnCleanup()
    {
        _manipulationState?.SetActive(false);
    }
}