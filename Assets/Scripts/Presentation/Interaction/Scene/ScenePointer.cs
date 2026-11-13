#nullable enable
using System;
using UnityEngine;

public sealed class ScenePointer : EventSubscriberBehaviour, IRunnable, IClickLeftSubscriber, IPointerSubscriber
{
    private IClickLeftHub _clickLeftHub = null!;
    private IPointerHub _pointerHub = null!;
    private UIPointerProbe _uiPointerProbe = null!;
    private HoverState _hoverState = null!;
    private CameraManipulationState _manipulationState = null!;
    private Func<Vector2, SceneTargetInfo> _classifyFunc = null!;
    private Action<SceneTargetInfo> _dispatchFunc = null!;
    private Func<bool> _blockFunc = null!;

    private bool _hasPointer;
    private Vector2 _lastPointerPosition;
    private bool _subscribed;

    public void Initialize(ScenePointerContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _clickLeftHub = context.ClickLeftHub ?? throw new ArgumentNullException(nameof(context.ClickLeftHub));
        _pointerHub = context.PointerHub ?? throw new ArgumentNullException(nameof(context.PointerHub));
        _uiPointerProbe = context.UIPointerProbe ?? throw new ArgumentNullException(nameof(context.UIPointerProbe));
        _hoverState = context.HoverState ?? throw new ArgumentNullException(nameof(context.HoverState));
        _manipulationState = context.ManipulationState ?? throw new ArgumentNullException(nameof(context.ManipulationState));

        if (context.ClassifyOverride != null)
        {
            _classifyFunc = context.ClassifyOverride;
        }
        else
        {
            var classifier = new SceneTargetClassifier(
                context.Camera,
                context.LayerCamera,
                context.ViewState,
                context.PointerLayers,
                context.MaxRayDistance,
                context.WWOffset);
            _classifyFunc = classifier.Classify;
        }

        _dispatchFunc = context.DispatchOverride ?? (_ => { });
        _blockFunc = context.BlockInteractionProvider ?? (() => false);
    }

    public void Run()
    {
        if (_subscribed) return;
        if (_classifyFunc == null) throw new InvalidOperationException("ScenePointer не инициализирован.");

        _clickLeftHub.RegisterSubscriber(this);
        AddSubscription(() => _clickLeftHub.UnregisterSubscriber(this));
        _pointerHub.RegisterSubscriber(this);
        AddSubscription(() => _pointerHub.UnregisterSubscriber(this));

        _manipulationState.Changed += OnManipulationChanged;
        AddSubscription(() => _manipulationState.Changed -= OnManipulationChanged);

        _subscribed = true;
    }

    public void Point(Vector2 pointer)
    {
        _lastPointerPosition = pointer;
        _hasPointer = true;

        if (_manipulationState.IsActive || _blockFunc() || _uiPointerProbe.IsPointerOverUI(pointer))
        {
            _hoverState.Clear();
            return;
        }

        UpdateHover();
    }

    public void TriggerClickLeft()
    {
        if (!_hasPointer) return;
        if (_manipulationState.IsActive) return;
        if (_blockFunc()) return;
        if (_uiPointerProbe.IsPointerOverUI(_lastPointerPosition)) return;

        var target = _classifyFunc(_lastPointerPosition);
        _dispatchFunc(target);
    }

    private void UpdateHover()
    {
        var target = _classifyFunc(_lastPointerPosition);
        IHoverable? hovered = target.Kind switch
        {
            InteractionTargetKind.Piece => target.Piece,
            InteractionTargetKind.MoveTarget => target.MoveTarget,
            InteractionTargetKind.AttackTarget => target.AttackTarget,
            InteractionTargetKind.TargetMarker => target.TargetMarker,
            _ => null,
        };
        _hoverState.Set(hovered);
    }

    private void OnManipulationChanged()
    {
        if (_manipulationState.IsActive) _hoverState.Clear();
    }
}