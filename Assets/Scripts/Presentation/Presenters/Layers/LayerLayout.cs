#nullable enable
using System.Collections.Generic;
using UnityEngine;

public static class LayerLayout
{
    public static bool IsVisible(int layerIndex, int activeIndex, int viewDir) => viewDir > 0 ? layerIndex >= activeIndex : layerIndex <= activeIndex;

    public static Vector3 ComputeBasePosition(int axis, int layerIndex, Square gridSize, float step, float layerGap)
    {
        int sizeAlongAxis = GridAxes.GetGridSize(axis, gridSize);
        float offset = (layerIndex - (sizeAlongAxis - 1) * 0.5f) * step * (1f + layerGap);

        if (axis == GridAxes.W)
            return new Vector3(offset, offset, offset);

        return GridAxes.ToWorldDirection(axis) * offset;
    }

    public static Vector3 ComputeLayerCameraUp(int axis, int viewDir, int rollDegrees)
    {
        Vector3 axisDir = GridAxes.ToWorldDirection(axis);
        Vector3 cameraDir = axisDir * viewDir;
        Vector3 referenceUp = Mathf.Abs(Vector3.Dot(cameraDir, Vector3.up)) > 0.99f
            ? Vector3.forward
            : Vector3.up;
        if (rollDegrees != 0) referenceUp = Quaternion.AngleAxis(rollDegrees, cameraDir) * referenceUp;
        Vector3 right = Vector3.Cross(referenceUp, cameraDir).normalized;
        return Vector3.Cross(cameraDir, right).normalized;
    }

    public static float ComputeApparentHeight(Vector3 dims, Quaternion tilt, Vector3 cameraUp)
    {
        Vector3 lx = tilt * Vector3.right;
        Vector3 ly = tilt * Vector3.up;
        Vector3 lz = tilt * Vector3.forward;
        return Mathf.Abs(Vector3.Dot(lx, cameraUp)) * dims.x
             + Mathf.Abs(Vector3.Dot(ly, cameraUp)) * dims.y
             + Mathf.Abs(Vector3.Dot(lz, cameraUp)) * dims.z;
    }
}