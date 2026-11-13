#nullable enable

using System;
using UnityEngine;

public static class GridAxes
{
    public const int X = 0;
    public const int Y = 1;
    public const int Z = 2;
    public const int W = 3;
    public const int Count = 4;

    private static readonly AxisInfo[] _axes = new AxisInfo[Count];
    private static readonly int[] _uiToInternal = new int[Count];

    static GridAxes()
    {
        Define(X, "X", gridComponent: 0, worldComponent: 0, uiIndex: 0);
        Define(Y, "Y", gridComponent: 1, worldComponent: 2, uiIndex: 1);
        Define(Z, "Z", gridComponent: 2, worldComponent: 1, uiIndex: 2);
        Define(W, "W", gridComponent: 3, worldComponent: -1, uiIndex: 3);
    }

    private static void Define(int index, string name, int gridComponent, int worldComponent, int uiIndex)
    {
        _axes[index] = new AxisInfo(index, name, gridComponent, worldComponent, uiIndex);
        _uiToInternal[uiIndex] = index;
    }

    public static string GetAxisName(int axis) => _axes[axis].Name;
    public static int ToUIAxis(int axis) => _axes[axis].UIIndex;
    public static int FromUIAxis(int uiAxis) => _uiToInternal[uiAxis];

    public static int GetGridCoordinate(int axis, Vector3Int gridPos)
    {
        if (axis == W) throw new ArgumentOutOfRangeException(nameof(axis),
            "W axis is not present in Vector3Int; use Square overload.");
        return gridPos[_axes[axis].GridComponent];
    }

    public static int GetGridSize(int axis, Vector3Int gridSize) => GetGridCoordinate(axis, gridSize);

    public static int GetGridCoordinate(int axis, Square gridPos) => axis switch
    {
        X => gridPos.X,
        Y => gridPos.Y,
        Z => gridPos.Z,
        W => gridPos.W,
        _ => throw new ArgumentOutOfRangeException(nameof(axis)),
    };

    public static int GetGridSize(int axis, Square gridSize) => GetGridCoordinate(axis, gridSize);

    public static Vector3 ToWorldDirection(int axis) => axis switch
    {
        X => Vector3.right,
        Y => Vector3.forward,
        Z => Vector3.up,
        W => new Vector3(1f, 1f, 1f).normalized,
        _ => throw new ArgumentOutOfRangeException(nameof(axis)),
    };
    public static int GetLayerIndex(int axis, Square pos, int wwOffset) => axis switch
    {
        X => pos.X,
        Y => pos.Y,
        Z => pos.Z,
        W => pos.W - wwOffset,
        _ => throw new ArgumentOutOfRangeException(nameof(axis)),
    };
    public static int GetWorldComponent(int axis) => axis switch
    {
        X => 0,
        Y => 2,
        Z => 1,
        _ => -1,
    };

    public static Vector3 GridToWorldLocal(Vector3Int gridPos, Vector3Int gridSize, float step)
    {
        Vector3 world = default;
        for (int i = 0; i < 3; i++)
        {
            var info = _axes[i];
            float g = gridPos[info.GridComponent];
            float size = gridSize[info.GridComponent];
            world[info.WorldComponent] = (g - (size - 1) * 0.5f) * step;
        }
        return world;
    }
    public static Vector3 GridToWorldLocal(Square gridPos, Square gridSize, float step, float wSpread = 1f, int wwOffset = 0)
    {
        Vector3 world = default;
        for (int i = 0; i < 3; i++)
        {
            var info = _axes[i];
            float g = GetSquareComponent(gridPos, info.GridComponent);
            float size = GetSquareComponent(gridSize, info.GridComponent);
            world[info.WorldComponent] = (g - (size - 1) * 0.5f) * step;
        }

        float worldCenter = wwOffset + (gridSize.W - 1) * 0.5f;
        float wOffset = (gridPos.W - worldCenter) * step * wSpread;
        world += new Vector3(wOffset, wOffset, wOffset);
        return world;
    }

    public static Vector3 GridSizeToWorldSize(Vector3Int gridSizeCount, float step)
    {
        Vector3 world = default;
        for (int i = 0; i < 3; i++)
        {
            var info = _axes[i];
            world[info.WorldComponent] = gridSizeCount[info.GridComponent] * step;
        }
        return world;
    }
    public static Vector3 ThinnedWorldSize(int axis, Vector3Int gridSize, float step)
    {
        if (axis == W) throw new ArgumentOutOfRangeException(nameof(axis),
            "W axis is not representable as a thinned Vector3 layer; use Square overload.");
        Vector3 worldSize = GridSizeToWorldSize(gridSize, step);
        worldSize[GetWorldComponent(axis)] = step;
        return worldSize;
    }
    public static Vector3 ThinnedWorldSize(int axis, Square gridSize, float step)
    {
        if (axis == W)
        {
            // Для W-оси слой — это полный X/Y/Z-куб.
            return GridSizeToWorldSize(
                new Vector3Int(gridSize.X, gridSize.Y, gridSize.Z), step);
        }
        var v3 = new Vector3Int(gridSize.X, gridSize.Y, gridSize.Z);
        return ThinnedWorldSize(axis, v3, step);
    }

    private static float GetSquareComponent(Square s, int component) => component switch
    {
        0 => s.X,
        1 => s.Y,
        2 => s.Z,
        3 => s.W,
        _ => 0f,
    };

    private readonly struct AxisInfo
    {
        public int Index { get; }
        public string Name { get; }
        public int GridComponent { get; }
        public int WorldComponent { get; }
        public int UIIndex { get; }

        public AxisInfo(int index, string name, int gridComponent, int worldComponent, int uiIndex)
        {
            Index = index;
            Name = name;
            GridComponent = gridComponent;
            WorldComponent = worldComponent;
            UIIndex = uiIndex;
        }
    }
}