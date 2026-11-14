#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class TargetTargetPresenter : MarkerPresenterBase
{
    public TargetTargetPresenter(
        LayerPresenter layerPresenter,
        LoaderOfBoard boardLoader,
        Material markerMaterial,
        float markerSizeFraction)
        : base(layerPresenter, boardLoader, markerMaterial, markerSizeFraction) { }

    protected override string MarkerNameSuffix => "TargetTarget";

    public void Show(IReadOnlyList<Square> cells)
    {
        Clear();
        if (cells == null) return;

        foreach (var cell in cells)
        {
            var result = CreateMarker(cell, block =>
            {
                block.SetFloat("_IsStay", 0f);
                block.SetFloat("_IsAttack", 0f);
            });
            if (result == null) continue;

            var (go, block) = result.Value;
            var marker = go.AddComponent<TargetTargetMarker>();
            marker.Initialize(cell, block);
        }
    }
}