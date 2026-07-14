#nullable enable

using System;
using VContainer.Unity;

public class GamePresenter : IDisposable
{
    private readonly IFlow _flow;
    private readonly IGameView _view;
    private readonly IEventBus _eventBus;
    private readonly IControllerOfCamera _camera;

    public GamePresenter(IFlow flow, IGameView view, IEventBus eventBus, IControllerOfCamera camera)
    {
        _flow = flow;
        _view = view;
        _eventBus = eventBus;
        _camera = camera;

        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceDeployedEvent<int>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Subscribe<PieceMovedEvent<int>>(OnPieceMoved);
        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }

    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        var newConfig = e.NewConfig;
        var axes = (MultipleAxes)newConfig.Axes;
        var box = new AxisAlignedBox(axes, newConfig.FieldSize, 0);
        _camera.FitToBoard(box.Bounds);
    }
    private void OnPieceDeployed(PieceDeployedEvent<Square> e) => _view.ShowPiece(e.Piece, e.Position);
    private void OnPieceDeployed(PieceDeployedEvent<int> e) => _view.ShowPieceInDeck(e.Piece, e.Position);
    private void OnPieceMoved(PieceMovedEvent<Square> e) => _view.UpdatePiecePosition(e.Piece, e.ToPosition);
    private void OnPieceMoved(PieceMovedEvent<int> e) => _view.UpdatePieceInDeckPosition(e.Piece, e.ToPosition);
    private void OnPieceDied(PieceDiedEvent e) => _view.HidePiece(e.Piece);

    public void Dispose()
    {
        _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceDeployedEvent<int>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceMovedEvent<int>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }
}