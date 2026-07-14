#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ControllerOfCamera : MonoBehaviour, IControllerOfCamera
{
    [SerializeField]
    private float _paddingFactor = 1.25f;
    private const float _minDistance = 1f;
    private Camera _camera = null!;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null) throw new NullReferenceException(nameof(_camera));
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
    private float ComputeDistance(float halfSize, float fovDegrees)
    {
        float fovRad = fovDegrees * Mathf.Deg2Rad / 2f;
        return halfSize / Mathf.Tan(fovRad) * _paddingFactor;
    }

    public void FitToBoard(IReadOnlyDictionary<Axis, (int Min, int Max)> bounds)
    {
        if (bounds.Count == 0) return;

        Vector3 center = ComputeCenter(bounds);
        float halfSize = ComputeHalfSize(bounds);
        float distance = Mathf.Max(ComputeDistance(halfSize, _camera.fieldOfView), _minDistance);

        transform.position = center + new Vector3(0f, distance, -distance);
        transform.LookAt(center);
    }
}