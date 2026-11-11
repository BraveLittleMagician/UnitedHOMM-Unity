#nullable enable

using System;
using UnityEngine;

public sealed class CameraMarkerPresenter
{
    private readonly Camera _camera;
    private readonly WorldToScreenMarker _cubeCenterMarker;
    private readonly WorldToScreenMarker _rotationCenterMarker;
    private readonly float _snapDistance;

    public CameraMarkerPresenter(
        Camera camera,
        WorldToScreenMarker cubeCenterMarker,
        WorldToScreenMarker rotationCenterMarker,
        float snapDistance)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _cubeCenterMarker = cubeCenterMarker ?? throw new ArgumentNullException(nameof(cubeCenterMarker));
        _rotationCenterMarker = rotationCenterMarker ?? throw new ArgumentNullException(nameof(rotationCenterMarker));
        _snapDistance = snapDistance;
    }

    public void Initialize()
    {
        _cubeCenterMarker.Initialize(_camera);
        _rotationCenterMarker.Initialize(_camera);
    }

    public void Update(Vector3 cubeCenter, Vector3 rotationCenter)
    {
        _cubeCenterMarker.SetWorldPosition(cubeCenter);
        _rotationCenterMarker.SetWorldPosition(rotationCenter);
    }

    public void SetVisible(bool visible)
    {
        _cubeCenterMarker.SetUserVisible(visible);
        _rotationCenterMarker.SetUserVisible(visible);
    }

    public bool IsCloseToCenterOnScreen(Vector3 worldA, Vector3 worldB)
    {
        Vector3 screenA = _camera.WorldToScreenPoint(worldA);
        Vector3 screenB = _camera.WorldToScreenPoint(worldB);
        if (screenA.z < 0 || screenB.z < 0) return false;
        Vector2 diff = new(screenA.x - screenB.x, screenA.y - screenB.y);
        return diff.magnitude <= _snapDistance;
    }
}