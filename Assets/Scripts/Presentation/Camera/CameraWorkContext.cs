#nullable enable
using UnityEngine;

public sealed class CameraWorkContext
{
    public LoaderOfBoard BoardLoader { get; init; } = null!;
    public Camera Camera { get; init; } = null!;
    public WorldToScreenMarker CubeCenterMarker { get; init; } = null!;
    public WorldToScreenMarker RotationCenterMarker { get; init; } = null!;
    public IHoldRightHub HoldRightHub { get; init; } = null!;
    public IHoldMiddleHub HoldMiddleHub { get; init; } = null!;
    public IDeltaHub DeltaHub { get; init; } = null!;
    public IScrollHub ScrollHub { get; init; } = null!;
    public CameraManipulationState ManipulationState { get; init; } = null!;
}