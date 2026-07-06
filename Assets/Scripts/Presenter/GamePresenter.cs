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

        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceDeployedEvent<int>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Subscribe<PieceMovedEvent<int>>(OnPieceMoved);
        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
    }

    private void OnPieceDeployed(PieceDeployedEvent<Square> e) => _view.ShowPiece(e.Piece, e.Position);
    private void OnPieceDeployed(PieceDeployedEvent<int> e) => _view.ShowPieceInDeck(e.Piece, e.Position);
    private void OnPieceMoved(PieceMovedEvent<Square> e) => _view.UpdatePiecePosition(e.Piece, e.ToPosition);
    private void OnPieceMoved(PieceMovedEvent<int> e) => _view.UpdatePieceInDeckPosition(e.Piece, e.ToPosition);
    private void OnPieceDied(PieceDiedEvent e) => _view.HidePiece(e.Piece);

    public void Start() { }
    public void Dispose()
    {
        _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceDeployedEvent<int>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceMovedEvent<int>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);
    }
}