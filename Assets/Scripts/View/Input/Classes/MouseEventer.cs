#nullable enable

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseEventer : MonoBehaviour
{
    [SerializeField] private InputActionAsset _action = null!;
    [SerializeField] private ClickLeft _leftClicker = null!;
    [SerializeField] private ClickRight _rightClicker = null!;
    [SerializeField] private Scroll _scroller = null!;
    [SerializeField] private Pointer _pointer = null!;
    [SerializeField] private Delta _pointerDelta = null!;
    [SerializeField] private HoldRight _rightHolder = null!;
    [SerializeField] private HoldMiddle _middleHolder = null!;

    private InputAction _leftClick = null!;
    private InputAction _rightClick = null!;
    private InputAction _wheel = null!;
    private InputAction _point = null!;
    private InputAction _delta = null!;
    private InputAction _rightHold = null!;
    private InputAction _middleHold = null!;
    private bool _subscribed = false;

    private void Awake()
    {
        if (_action == null) throw new ArgumentNullException(nameof(_action));
        if (_leftClicker == null) throw new ArgumentNullException(nameof(_leftClicker));
        if (_rightClicker == null) throw new ArgumentNullException(nameof(_rightClicker));
        if (_scroller == null) throw new ArgumentNullException(nameof(_scroller));
        if (_pointer == null) throw new ArgumentNullException(nameof(_pointer));
        if (_pointerDelta == null) throw new ArgumentNullException(nameof(_pointerDelta));
        if (_rightHolder == null) throw new ArgumentNullException(nameof(_rightHolder));
        if (_middleHolder == null) throw new ArgumentNullException(nameof(_middleHolder));

        _leftClick = _action.FindAction("UI/Click") ?? throw new ArgumentNullException(nameof(_leftClick));
        _rightClick = _action.FindAction("UI/RightClick") ?? throw new ArgumentNullException(nameof(_rightClick));
        _rightHold = _action.FindAction("UI/RightHold") ?? throw new ArgumentNullException(nameof(_rightHold));
        _wheel = _action.FindAction("UI/ScrollWheel") ?? throw new ArgumentNullException(nameof(_wheel));
        _point = _action.FindAction("UI/Point") ?? throw new ArgumentNullException(nameof(_point));
        _delta = _action.FindAction("UI/Delta") ?? throw new ArgumentNullException(nameof(_delta));
        _middleHold = _action.FindAction("UI/MiddleHold") ?? throw new ArgumentNullException(nameof(_middleHold));

        SubscribeEvents();
    }
    private void OnDestroy() => UnsubscribeEvents();

    private void SubscribeEvents()
    {
        _leftClick.Enable();
        _rightClick.Enable();
        _rightHold.Enable();
        _middleHold.Enable();
        _wheel.Enable();
        _point.Enable();
        _delta.Enable();

        _leftClick.performed += OnLeftClick;
        _rightClick.performed += OnRightClick;
        _rightHold.performed += OnRightHold;
        _rightHold.canceled += OnRightHoldCanceled;
        _middleHold.performed += OnMiddleHold;
        _middleHold.canceled += OnMiddleHoldCanceled;
        _wheel.performed += OnScroll;
        _point.performed += OnPoint;
        _delta.performed += OnDelta;

        _subscribed = true;
    }
    private void UnsubscribeEvents()
    {
        if (!_subscribed) return;

        _leftClick.performed -= OnLeftClick;
        _rightClick.performed -= OnRightClick;
        _rightHold.performed -= OnRightHold;
        _rightHold.canceled -= OnRightHoldCanceled;
        _middleHold.performed -= OnMiddleHold;
        _middleHold.canceled -= OnMiddleHoldCanceled;
        _wheel.performed -= OnScroll;
        _point.performed -= OnPoint;
        _delta.performed -= OnDelta;

        _leftClick.Disable();
        _rightClick.Disable();
        _rightHold.Disable();
        _middleHold.Disable();
        _wheel.Disable();
        _point.Disable();
        _delta.Disable();

        _subscribed = false;
    }
    private void OnLeftClick(InputAction.CallbackContext _)
    {
        _leftClicker.TriggerClickLeft();
    }
    private void OnRightClick(InputAction.CallbackContext _)
    {
        _rightClicker.TriggerClickRight();
    }
    private void OnRightHold(InputAction.CallbackContext _)
    {
        _rightHolder.TriggerHoldRightStarted();
    }
    private void OnRightHoldCanceled(InputAction.CallbackContext _)
    {
        _rightHolder.TriggerHoldRightCanceled();
    }
    private void OnMiddleHold(InputAction.CallbackContext _)
    {
        _middleHolder.TriggerHoldMiddleStarted();
    }
    private void OnMiddleHoldCanceled(InputAction.CallbackContext _)
    {
        _middleHolder.TriggerHoldMiddleCanceled();
    }
    private void OnScroll(InputAction.CallbackContext ctx)
    {
        _scroller.TriggerScroll(ctx.ReadValue<Vector2>().y);
    }
    private void OnPoint(InputAction.CallbackContext ctx)
    {
        _pointer.TriggerPoint(ctx.ReadValue<Vector2>());
    }
    private void OnDelta(InputAction.CallbackContext ctx)
    {
        _pointerDelta.TriggerDelta(ctx.ReadValue<Vector2>());
    }
}