#nullable enable
using System.Collections.Generic;
using UnityEngine;

public sealed class BoardViewHierarchy
{
    public GameObject? PiecesSource { get; }
    public GameObject? MoveTargetsSource { get; }
    public List<LayerContainer> Containers { get; }

    public BoardViewHierarchy(
        GameObject? piecesSource,
        GameObject? moveTargetsSource,
        List<LayerContainer> containers)
    {
        PiecesSource = piecesSource;
        MoveTargetsSource = moveTargetsSource;
        Containers = containers;
    }

    public static BoardViewHierarchy Empty { get; } =
        new(null, null, new List<LayerContainer>());
}