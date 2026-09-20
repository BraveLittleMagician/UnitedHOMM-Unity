#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraWork : MonoBehaviour, ICameraWork
{
    [SerializeField] private float _paddingFactor2D = 0.75f;
    [SerializeField] private float _paddingFactor3D = 1.5f;
    [SerializeField] private float _orthographicZOffset = -10f;
    private const float _minDistance = 1f;
    private Camera _camera = null!;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null) throw new ArgumentNullException(nameof(_camera));
    }

    private static Vector3 ComputeCenter(IReadOnlyDictionary<Axis, (int Min, int Max)> bounds)
    {
        float cx = bounds.TryGetValue(Axis.X, out var x) ? (x.Min + x.Max) / 2f : 0f;
        float cy = bounds.TryGetValue(Axis.Y, out var y) ? (y.Min + y.Max) / 2f : 0f;
        float cz = bounds.TryGetValue(Axis.Z, out var z) ? (z.Min + z.Max) / 2f : 0f;
        return new Vector3(cx, cy, cz);
    }
    private static float ComputeHalfSize(IReadOnlyDictionary<Axis, (int Min, int Max)> bounds)
    {
        float maxExtent = 0f;
        foreach (var (Min, Max) in bounds.Values)
            maxExtent = Mathf.Max(maxExtent, (Max - Min) / 2f);
        return maxExtent;
    }
    private float ComputeDistance(float halfSize, float fovDegrees, float paddingFactor)
    {
        float fovRad = fovDegrees * Mathf.Deg2Rad / 2f;
        return halfSize / Mathf.Tan(fovRad) * paddingFactor;
    }

    public void FitToBoard(IReadOnlyDictionary<Axis, (int Min, int Max)> bounds)
    {
        if (bounds.Count == 0) return;

        Vector3 center = ComputeCenter(bounds);
        float halfSize = ComputeHalfSize(bounds);
        bool is3D = bounds.ContainsKey(Axis.Z);
        float effectivePadding = is3D ? _paddingFactor3D : _paddingFactor2D;

        if (_camera.orthographic)
        {
            float orthoSize = halfSize * effectivePadding;
            orthoSize = Mathf.Max(orthoSize, 0.1f);
            _camera.orthographicSize = orthoSize;
            transform.position = center + new Vector3(0f, 0f, _orthographicZOffset);
            transform.LookAt(center);
        }
        else
        {
            float distance = Mathf.Max(ComputeDistance(halfSize, _camera.fieldOfView, effectivePadding), _minDistance);
            transform.position = center + new Vector3(0f, distance, -distance);
            transform.LookAt(center);
        }
    }
    public void Initialize() {}
}