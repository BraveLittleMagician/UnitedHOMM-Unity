#nullable enable

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseEventer : EventSubscriberBehaviour, IRunnable
{
    private IClickLeftHub _leftClicker = null!;
    private IClickRightHub _rightClicker = null!;
    private IScrollHub _scroller = null!;
    private IPointerHub _pointer = null!;
    private IDeltaHub _pointerDelta = null!;
    private IHoldRightHub _rightHolder = null!;
    private IHoldMiddleHub _middleHolder = null!;
    
    private InputActionAsset _action = null!;

    private InputAction _leftClick = null!;
    private InputAction _rightClick = null!;
    private InputAction _wheel = null!;
    private InputAction _point = null!;
    private InputAction _delta = null!;
    private InputAction _rightHold = null!;
    private InputAction _middleHold = null!;
    private bool _subscribed = false;

    private void EnableAll()
    {
        _leftClick.Enable();
        _rightClick.Enable();
        _rightHold.Enable();
        _middleHold.Enable();
        _wheel.Enable();
        _point.Enable();
        _delta.Enable();
    }
    private void DisableAll()
    {
        _leftClick.Disable();
        _rightClick.Disable();
        _rightHold.Disable();
        _middleHold.Disable();
        _wheel.Disable();
        _point.Disable();
        _delta.Disable();
    }
    private void SubscribeAll()
    {
        _leftClick.performed += OnLeftClick;
        _rightClick.performed += OnRightClick;
        _rightHold.performed += OnRightHold;
        _rightHold.canceled += OnRightHoldCanceled;
        _middleHold.performed += OnMiddleHold;
        _middleHold.canceled += OnMiddleHoldCanceled;
        _wheel.performed += OnScroll;
        _point.performed += OnPoint;
        _delta.performed += OnDelta;
    }
    private void UnsubscribeAll()
    {
        _leftClick.performed -= OnLeftClick;
        _rightClick.performed -= OnRightClick;
        _rightHold.performed -= OnRightHold;
        _rightHold.canceled -= OnRightHoldCanceled;
        _middleHold.performed -= OnMiddleHold;
        _middleHold.canceled -= OnMiddleHoldCanceled;
        _wheel.performed -= OnScroll;
        _point.performed -= OnPoint;
        _delta.performed -= OnDelta;
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

    public void Initialize(InputActionAsset action, IClickLeftHub leftClicker, IClickRightHub rightClicker, IHoldRightHub rightHolder, IHoldMiddleHub middleHolder, IScrollHub scroller, IPointerHub pointer, IDeltaHub pointerDelta)
    {
        _action = action != null ? action : throw new ArgumentNullException(nameof(action));
        _leftClicker = leftClicker ?? throw new ArgumentNullException(nameof(leftClicker));
        _rightClicker = rightClicker ?? throw new ArgumentNullException(nameof(rightClicker));
        _rightHolder = rightHolder ?? throw new ArgumentNullException(nameof(rightHolder));
        _middleHolder = middleHolder ?? throw new ArgumentNullException(nameof(middleHolder));
        _scroller = scroller ?? throw new ArgumentNullException(nameof(scroller));
        _pointer = pointer ?? throw new ArgumentNullException(nameof(pointer));
        _pointerDelta = pointerDelta ?? throw new ArgumentNullException(nameof(pointerDelta));
    }
    public void Run()
    {
        if (_action == null) throw new InvalidOperationException("MouseEventer не инициализирован.");

        _leftClick = _action.FindAction("UI/Click") ?? throw new ArgumentNullException(nameof(_leftClick));
        _rightClick = _action.FindAction("UI/RightClick") ?? throw new ArgumentNullException(nameof(_rightClick));
        _rightHold = _action.FindAction("UI/RightHold") ?? throw new ArgumentNullException(nameof(_rightHold));
        _middleHold = _action.FindAction("UI/MiddleHold") ?? throw new ArgumentNullException(nameof(_middleHold));
        _wheel = _action.FindAction("UI/ScrollWheel") ?? throw new ArgumentNullException(nameof(_wheel));
        _point = _action.FindAction("UI/Point") ?? throw new ArgumentNullException(nameof(_point));
        _delta = _action.FindAction("UI/Delta") ?? throw new ArgumentNullException(nameof(_delta));

        EnableAll();
        SubscribeAll();
        _subscribed = true;
        AddSubscription(Dispose);
    }
    public void Dispose()
    {
        if (!_subscribed) return;
        UnsubscribeAll();
        DisableAll();
        _subscribed = false;
    }
}