#nullable enable

using UnityEngine;

public sealed class CameraManipulationModel
{
    private readonly float _rotationSpeed;
    private readonly float _panSpeed;
    private readonly float _zoomSpeed;
    private readonly float _minDistance;
    private readonly float _maxDistance;

    private float _x;
    private float _y;
    private float _roll;
    private float _distance;
    private Vector3 _targetOffset = Vector3.zero;

    public CameraManipulationModel(
        float rotationSpeed,
        float panSpeed,
        float zoomSpeed,
        float minDistance,
        float maxDistance)
    {
        _rotationSpeed = rotationSpeed;
        _panSpeed = panSpeed;
        _zoomSpeed = zoomSpeed;
        _minDistance = minDistance;
        _maxDistance = maxDistance;
    }

    public Vector3 TargetOffset => _targetOffset;
    public Quaternion Rotation => Quaternion.Euler(_y, _x, _roll);

    public void Initialize(float x, float y, float roll, float distance)
    {
        _x = x;
        _y = y;
        _roll = roll;
        _distance = Mathf.Clamp(distance, _minDistance, _maxDistance);
    }

    public void ApplyOrbitDelta(Vector2 delta)
    {
        _x += delta.x * _rotationSpeed * 0.02f;
        _y -= delta.y * _rotationSpeed * 0.02f;
        _y = Mathf.Clamp(_y, -90f, 90f);
    }

    public void ApplyPanDelta(Vector2 delta, Vector3 right, Vector3 up)
    {
        _targetOffset -= _panSpeed * 0.02f * (right * delta.x + up * delta.y);
    }

    public void ApplyZoom(float scrollDelta)
    {
        _distance -= scrollDelta * _zoomSpeed;
        _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);
    }

    public void SnapPanToZero() => _targetOffset = Vector3.zero;

    public Vector3 GetPosition(Vector3 pivot)
    {
        return Rotation * new Vector3(0f, 0f, -_distance) + pivot;
    }
}