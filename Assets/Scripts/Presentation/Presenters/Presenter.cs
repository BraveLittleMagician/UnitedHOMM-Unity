#nullable enable

using System;

public class Presenter : IDisposable
{
    private readonly IEventBus _eventBus;
    private readonly CameraWork _camera;
    private ConfigOfBoard _currentConfig;

    public Presenter(IEventBus eventBus, CameraWork camera, ConfigOfBoard initialConfig)
    {
        _eventBus = eventBus;
        _camera = camera;
        _currentConfig = initialConfig;

        _eventBus.Subscribe<PieceSelectedEvent<Square>>(OnPieceSelectedEvent);
        _eventBus.Subscribe<PieceSelectedEvent<int>>(OnPieceSelectedEvent);
        _eventBus.Subscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceDeployedEvent<int>>(OnPieceDeployed);
        _eventBus.Subscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Subscribe<PieceMovedEvent<int>>(OnPieceMoved);
        _eventBus.Subscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Subscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }

    private void OnPieceSelectedEvent(PieceSelectedEvent<Square> e) { }
    private void OnPieceSelectedEvent(PieceSelectedEvent<int> e) { }
    private void OnBoardConfigChanged(BoardConfigChangedEvent e)
    {
        _currentConfig = e.NewConfig;
    }
    private void OnPieceDeployed(PieceDeployedEvent<Square> e) { }
    private void OnPieceDeployed(PieceDeployedEvent<int> e) { }
    private void OnPieceMoved(PieceMovedEvent<Square> e) { }
    private void OnPieceMoved(PieceMovedEvent<int> e) { }
    private void OnPieceDied(PieceDiedEvent e) { }

    public void Start()
    {
    }
    public void Dispose()
    {
        _eventBus.Unsubscribe<PieceSelectedEvent<Square>>(OnPieceSelectedEvent);
        _eventBus.Unsubscribe<PieceSelectedEvent<int>>(OnPieceSelectedEvent);
        _eventBus.Unsubscribe<PieceDeployedEvent<Square>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceDeployedEvent<int>>(OnPieceDeployed);
        _eventBus.Unsubscribe<PieceMovedEvent<Square>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceMovedEvent<int>>(OnPieceMoved);
        _eventBus.Unsubscribe<PieceDiedEvent>(OnPieceDied);
        _eventBus.Unsubscribe<BoardConfigChangedEvent>(OnBoardConfigChanged);
    }
}