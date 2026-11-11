#nullable enable

using System;
using UnityEngine;

public sealed class ScenePointerContext
{
    public IClickLeftHub ClickLeftHub { get; init; } = null!;
    public IPointerHub PointerHub { get; init; } = null!;
    public UIPointerProbe UIPointerProbe { get; init; } = null!;
    public Camera Camera { get; init; } = null!;
    public Camera LayerCamera { get; init; } = null!;
    public ViewState ViewState { get; init; } = null!;
    public HoverState HoverState { get; init; } = null!;
    public CameraManipulationState ManipulationState { get; init; } = null!;
    public LayerMask PointerLayers { get; init; } = ~0;
    public float MaxRayDistance { get; init; } = 1000f;

    public Func<Vector2, SceneTargetInfo>? ClassifyOverride { get; init; }
    public Action<SceneTargetInfo>? DispatchOverride { get; init; }

    public Func<bool>? BlockInteractionProvider { get; init; }
}