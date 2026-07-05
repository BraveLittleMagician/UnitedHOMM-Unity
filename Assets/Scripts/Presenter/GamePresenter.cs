#nullable enable

using System;
using VContainer.Unity;

public class GamePresenter : IStartable, IDisposable
{
    private readonly IFlow _flow;
    private readonly IGameView _view;
    private readonly IEventBus _eventBus;

    public GamePresenter(IFlow flow, IGameView view, IEventBus eventBus)
    {
        _flow = flow;
        _view = view;
        _eventBus = eventBus;

        _eventBus.Subscribe<PieceDeployedEvent>(OnPieceDeployed);
        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
    }

    private void OnPieceDeployed(PieceDeployedEvent e)
    {
        // Определяем позицию фигуры (нужно получить через Registry)
        // Пока используем заглушку
        var pos = new Square { X = 0, Y = 0 };
        _view.ShowPiece(e.Piece, pos);
    }

    private void OnPieceDied(PieceDiedEvent e)
    {
        _view.HidePiece(e.Piece);
    }

    private void OnPieceMoved(PieceMovedEvent<Square> e)
    {
        _view.UpdatePiecePosition(e.Piece, e.ToPosition);
    }

    public void Start() { }

    public void Dispose()
    {
        _eventBus.Unsubscribe<PieceDeployedEvent>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved);
    }
}