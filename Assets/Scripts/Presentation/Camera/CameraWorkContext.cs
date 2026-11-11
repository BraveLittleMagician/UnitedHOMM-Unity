#nullable enable

using UnityEngine;

public sealed class CameraWorkContext
{
    public LoaderOfBoard BoardLoader { get; init; } = null!;
    public Camera Camera { get; init; } = null!;
    public WorldToScreenMarker CubeCenterMarker { get; init; } = null!;
    public WorldToScreenMarker RotationCenterMarker { get; init; } = null!;
    public HoldRight HoldRightHub { get; init; } = null!;
    public HoldMiddle HoldMiddleHub { get; init; } = null!;
    public Delta DeltaHub { get; init; } = null!;
    public Scroll ScrollHub { get; init; } = null!;
    public CameraManipulationState ManipulationState { get; init; } = null!;
}