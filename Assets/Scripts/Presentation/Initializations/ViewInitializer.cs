#nullable enable

using System;
using System.Collections.Generic;

public sealed class ViewInitializer : IViewInitializer
{
    private readonly IGameView _gameView;
    private readonly ICamera _camera;
    private readonly Board _board;

    public ViewInitializer(IGameView gameView, ICamera camera, Board board)
    {
        _gameView = gameView ?? throw new ArgumentNullException(nameof(gameView));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _board = board ?? throw new ArgumentNullException(nameof(board));
    }

    public void InitializeView()
    {
        IReadOnlyDictionary<Axis, (int Min, int Max)> bounds = _board.Bounds;

        _gameView.BuildGrid(bounds);
        _camera.FitToBoard(bounds);
    }
}