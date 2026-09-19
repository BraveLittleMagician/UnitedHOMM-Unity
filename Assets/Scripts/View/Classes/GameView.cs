#nullable enable

using System;
using UnityEngine;

public class GameView : MonoBehaviour, IGameView
{
    [SerializeField] private GridRenderer _gridRenderer = null!;

    private void Awake()
    {
        if (_gridRenderer == null) throw new ArgumentNullException(nameof(_gridRenderer));
    }
    private void OnBoardSizeInputChanged(int newSize)
    {
        BoardSizeInputChanged?.Invoke(newSize);
    }

    public void BuildGrid(AxisAlignedBox box) => _gridRenderer.BuildGrid(box);
    public void SetBoardSize(int size)
    {
    }

    public event Action<int>? BoardSizeInputChanged;
}