#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BoardViewFactory
{
    private static GameObject CreateSource(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        return go;
    }

    public void Destroy(BoardViewHierarchy? view)
    {
        if (view == null) return;
        if (view.PiecesSource != null) UnityObjectCleanup.DestroySafe(view.PiecesSource);
        if (view.MoveTargetsSource != null) UnityObjectCleanup.DestroySafe(view.MoveTargetsSource);
        view.Containers.Clear();
    }

    public BoardViewHierarchy Create(BoardViewFactoryContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        GameObject piecesSource = CreateSource("PiecesSource", ctx.Parent);
        GameObject moveTargetsSource = CreateSource("MoveTargetsSource", ctx.Parent);

        int layerCount = GridAxes.GetGridSize(ctx.Axis, ctx.GridSize);
        var containers = new List<LayerContainer>(layerCount);

        for (int i = 0; i < layerCount; i++)
        {
            Vector3 basePos = LayerLayout.ComputeBasePosition(
                ctx.Axis, i, ctx.GridSize, ctx.Step, ctx.LayerGap);

            var piecesLayer = new GameObject($"Layer_{ctx.AxisName}_{i}");
            piecesLayer.transform.SetParent(piecesSource.transform, false);
            piecesLayer.SetLayerRecursively(ctx.LayersLayerIndex);
            var container = piecesLayer.AddComponent<LayerContainer>();
            container.Initialize(ctx.Axis, i, ctx.GridSize, ctx.Step, basePos, ctx.LayerMaterial);
            containers.Add(container);

            var targetsLayer = new GameObject($"MoveTargets_{ctx.AxisName}_{i}");
            targetsLayer.transform.SetParent(moveTargetsSource.transform, false);
            targetsLayer.transform.SetLocalPositionAndRotation(basePos, Quaternion.identity);
            targetsLayer.SetLayerRecursively(ctx.LayersLayerIndex);
            container.AttachTargetTwin(targetsLayer.transform);
        }

        return new BoardViewHierarchy(piecesSource, moveTargetsSource, containers);
    }
}