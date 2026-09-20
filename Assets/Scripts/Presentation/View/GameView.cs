#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class GameView : MonoBehaviour, IGameView
{
    [SerializeField] private GridRenderer _gridRenderer = null!;

    private void Awake()
    {
        if (_gridRenderer == null) throw new ArgumentNullException(nameof(_gridRenderer));
    }
    public void BuildGrid(IReadOnlyDictionary<Axis, (int Min, int Max)> box) => _gridRenderer.BuildGrid(box);
    public void SetBoardSize(int size) { }
}