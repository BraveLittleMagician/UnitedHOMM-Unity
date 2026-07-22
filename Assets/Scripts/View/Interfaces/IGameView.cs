#nullable enable

using System;

public interface IGameView
{
    void BuildGrid(AxisAlignedBox box);
    void SetBoardSize(int size);

    event Action<int>? BoardSizeInputChanged;
}