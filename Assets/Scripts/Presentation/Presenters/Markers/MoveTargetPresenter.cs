#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MoveTargetPresenter : MarkerPresenterBase
{
    public MoveTargetPresenter(
        LayerPresenter layerPresenter,
        LoaderOfBoard boardLoader,
        Material markerMaterial,
        float markerSizeFraction)
        : base(layerPresenter, boardLoader, markerMaterial, markerSizeFraction) { }

    protected override string MarkerNameSuffix => "MoveTarget";

    public void Show(IReadOnlyList<ShapeCell> shape)
    {
        if (shape == null) throw new ArgumentNullException(nameof(shape));

        foreach (var shapeCell in shape)
        {
            bool isStay = shapeCell.Stayable == Stayable.Stay;
            var result = CreateMarker(shapeCell.Cell, block =>
            {
                block.SetFloat("_IsStay", isStay ? 1f : 0f);
                block.SetFloat("_IsAttack", 0f);
            });
            if (result == null) continue;

            var (go, block) = result.Value;
            var marker = go.AddComponent<MoveTargetMarker>();
            marker.Initialize(shapeCell.Cell, shapeCell.Stayable, block);
        }
    }
}