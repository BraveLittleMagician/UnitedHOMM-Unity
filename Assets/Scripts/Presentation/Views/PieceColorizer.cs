#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public static class PieceColorizer
{
    private static readonly List<Material> _singleMaterial = new(1);

    public static void ApplyInitialColor(GameObject pieceObj, InfoOfPiece info, Material pieceMaterial, IndexOfPlayer player, int teamCount, int subteamCount)
    {
        var renderers = pieceObj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) throw new InvalidOperationException(
                $"У '{pieceObj.name}' нет ни одного Renderer — цвет применить нельзя.");

        var (hue, lightness) = GetHL(player, teamCount, subteamCount);
        Color targetColor = ColorExtensions.HslToColor(hue, 1, lightness);

        var block = new MaterialPropertyBlock();
        block.SetColor("_NormalColor", targetColor);
        block.SetFloat("_IsHighlighted", 0f);
        block.SetFloat("_IsSelected", 0f);
        block.SetFloat("_IsHovered", 0f);

        _singleMaterial.Clear();
        _singleMaterial.Add(pieceMaterial);

        foreach (var rend in renderers)
        {
            rend.SetMaterials(_singleMaterial);
            rend.SetPropertyBlock(block);
        }

        info.Block = block;
    }

    public static void SetVisualState(InfoOfPiece info, bool selected, bool highlighted, bool hovered)
    {
        var block = info.Block;
        if (block == null) return;

        block.SetFloat("_IsSelected", selected ? 1f : 0f);
        block.SetFloat("_IsHighlighted", highlighted ? 1f : 0f);
        block.SetFloat("_IsHovered", hovered ? 1f : 0f);

        foreach (var r in info.GetComponentsInChildren<Renderer>())
            r.SetPropertyBlock(block);
    }

    private static (double hue, double lightness) GetHL(IndexOfPlayer index, int teamCount, int subteamCount)
    {
        int half = teamCount / 2;

        double hueOffset = 60.0;
        double step = 360.0 / Math.Max(teamCount, 1);

        double v = subteamCount <= 1 ? 0.0 : index.IndexOfPlayerOnSide / (double)(subteamCount - 1);

        double hue;
        double u;
        double lightness;

        if (index.IndexOfSide < half)
        {
            u = half <= 1 ? 0.0 : index.IndexOfSide / (double)(half - 1);

            hue = hueOffset - index.IndexOfSide * step;
            lightness = 1.0 - (u + v) * 0.25;
        }
        else
        {
            int localIndex = index.IndexOfSide - half;
            int localCount = teamCount - half;

            int reversedLocalIndex = localCount - 1 - localIndex;

            u = localCount <= 1 ? 1.0 : reversedLocalIndex / (double)(localCount - 1);

            double vRev = 1.0 - v;

            hue = 240.0 - localIndex * step;
            lightness = 0.5 - (u + vRev) * 0.25;
        }

        return (hue, lightness);
    }
}