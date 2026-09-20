#nullable enable

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseEventer : MonoBehaviour
{
    [SerializeField] private InputActionAsset _action = null!;

    [SerializeField] private ClickLeft _leftClickerBehaviour = null!;
    [SerializeField] private ClickRight _rightClickerBehaviour = null!;
    [SerializeField] private Scroll _scrollerBehaviour = null!;
    [SerializeField] private Pointer _pointerBehaviour = null!;
    [SerializeField] private Delta _pointerDeltaBehaviour = null!;
    [SerializeField] private HoldRight _rightHolderBehaviour = null!;
    [SerializeField] private HoldMiddle _middleHolderBehaviour = null!;

    private IClickLeftHub _leftClicker = null!;
    private IClickRightHub _rightClicker = null!;
    private IScrollHub _scroller = null!;
    private IPointerHub _pointer = null!;
    private IDeltaHub _pointerDelta = null!;
    private IHoldRightHub _rightHolder = null!;
    private IHoldMiddleHub _middleHolder = null!;

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

        _leftClicker = _leftClickerBehaviour as IClickLeftHub ?? throw new InvalidCastException($"{nameof(_leftClickerBehaviour)} не реализует {nameof(IClickLeftHub)}");
        _rightClicker = _rightClickerBehaviour as IClickRightHub ?? throw new InvalidCastException($"{nameof(_rightClickerBehaviour)} не реализует {nameof(IClickRightHub)}");
        _scroller = _scrollerBehaviour as IScrollHub ?? throw new InvalidCastException($"{nameof(_scrollerBehaviour)} не реализует {nameof(IScrollHub)}");
        _pointer = _pointerBehaviour as IPointerHub ?? throw new InvalidCastException($"{nameof(_pointerBehaviour)} не реализует {nameof(IPointerHub)}");
        _pointerDelta = _pointerDeltaBehaviour as IDeltaHub ?? throw new InvalidCastException($"{nameof(_pointerDeltaBehaviour)} не реализует {nameof(IDeltaHub)}");
        _rightHolder = _rightHolderBehaviour as IHoldRightHub ?? throw new InvalidCastException($"{nameof(_rightHolderBehaviour)} не реализует {nameof(IHoldRightHub)}");
        _middleHolder = _middleHolderBehaviour as IHoldMiddleHub ?? throw new InvalidCastException($"{nameof(_middleHolderBehaviour)} не реализует {nameof(IHoldMiddleHub)}");

        if (_leftClickerBehaviour == null) throw new ArgumentNullException(nameof(_leftClickerBehaviour));
        if (_rightClickerBehaviour == null) throw new ArgumentNullException(nameof(_rightClickerBehaviour));
        if (_scrollerBehaviour == null) throw new ArgumentNullException(nameof(_scrollerBehaviour));
        if (_pointerBehaviour == null) throw new ArgumentNullException(nameof(_pointerBehaviour));
        if (_pointerDeltaBehaviour == null) throw new ArgumentNullException(nameof(_pointerDeltaBehaviour));
        if (_rightHolderBehaviour == null) throw new ArgumentNullException(nameof(_rightHolderBehaviour));
        if (_middleHolderBehaviour == null) throw new ArgumentNullException(nameof(_middleHolderBehaviour));

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