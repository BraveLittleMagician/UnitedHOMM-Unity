#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AttackTargetPresenter : MarkerPresenterBase
{
    private readonly IReadOnlyList<ICellActionProvider> _providers;
    private readonly HashSet<Square> _cellsBuffer = new();

    public AttackTargetPresenter(
        LayerPresenter layerPresenter,
        LoaderOfBoard boardLoader,
        Material markerMaterial,
        IReadOnlyList<ICellActionProvider> providers,
        float markerSizeFraction)
        : base(layerPresenter, boardLoader, markerMaterial, markerSizeFraction)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
    }

    protected override string MarkerNameSuffix => "Attack";

    public void Show(IPiece attacker)
    {
        if (attacker == null) throw new ArgumentNullException(nameof(attacker));

        Clear();
        _cellsBuffer.Clear();

        for (int i = 0; i < _providers.Count; i++)
        {
            var providerCells = _providers[i].GetActionCells(attacker);
            for (int j = 0; j < providerCells.Count; j++)
                _cellsBuffer.Add(providerCells[j]);
        }

        foreach (var cell in _cellsBuffer)
        {
            var result = CreateMarker(cell, block =>
            {
                block.SetFloat("_IsStay", 0f);
                block.SetFloat("_IsAttack", 1f);
            });
            if (result == null) continue;

            var (go, block) = result.Value;
            var marker = go.AddComponent<AttackMarker>();
            marker.Initialize(cell, block);
        }
    }
}